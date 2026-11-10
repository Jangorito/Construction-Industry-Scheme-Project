import { HttpErrorResponse } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, catchError, forkJoin, map, of, switchMap, tap } from 'rxjs';

import { UPLOAD_POOL } from '../data/upload-pool';
import { ApiLabel } from '../models/classify-api.model';
import { Classification, Invoice, InvoiceStatus } from '../models/invoice.model';
import { StoredInvoiceDto } from '../models/stored-invoice.model';
import { UnclassifiedInvoice } from '../models/unclassified-invoice.model';
import { ClassifyApiService } from '../services/classify-api.service';
import { ExtractApiService } from '../services/extract-api.service';
import { InvoiceApiService } from '../services/invoice-api.service';
import {
  extractErrorMessage,
  failedExtractionPlaceholder,
  mergeClassification,
  toClassifyRequest,
  toUnclassifiedInvoice,
} from '../utils/classification-mapper';
import { SortDir, SortKey, buildDashboardAggregates, buildGridRows, computeAll } from '../utils/invoice-calculations';
import { toInvoice } from '../utils/stored-invoice-mapper';

export interface EditDraft {
  classification: Classification;
  reason: string;
}

export interface EditingTarget {
  invoiceId: string;
  lineIndex: number;
}

export type BatchResult =
  | { outcome: 'success'; raw: UnclassifiedInvoice; invoice: Invoice }
  | { outcome: 'error'; raw: UnclassifiedInvoice; message: string };

function isSuccess(r: BatchResult): r is Extract<BatchResult, { outcome: 'success' }> {
  return r.outcome === 'success';
}

// Single source of truth for the whole app: the invoice list, the upload
// pool/queue, and UI state (search/filter/sort/editing). Mirrors the
// mockup's "flat state, derived values recomputed fresh" pattern using
// signals instead of a custom re-render loop.
@Injectable({ providedIn: 'root' })
export class InvoiceStore {
  private readonly classifyApi = inject(ClassifyApiService);
  private readonly extractApi = inject(ExtractApiService);
  private readonly invoiceApi = inject(InvoiceApiService);

  // Starts empty, populated by loadInvoices(). Never observed empty by a
  // routed component: app.config.ts's provideAppInitializer awaits
  // loadInvoices() once before the router activates any route.
  private readonly invoicesSig = signal<Invoice[]>([]);
  private readonly loadErrorSig = signal<string | null>(null);

  private readonly poolSig = signal<UnclassifiedInvoice[]>(UPLOAD_POOL);
  private readonly queuedFilesSig = signal<File[]>([]);
  private readonly processingSig = signal(false);
  private readonly batchResultsSig = signal<BatchResult[] | null>(null);

  private readonly editingSig = signal<EditingTarget | null>(null);
  private readonly editDraftSig = signal<EditDraft>({ classification: 'construction', reason: '' });
  private readonly savingSig = signal(false);
  private readonly saveErrorSig = signal<string | null>(null);

  private readonly searchSig = signal('');
  private readonly statusFilterSig = signal<'all' | InvoiceStatus>('all');
  private readonly sortKeySig = signal<SortKey>('date');
  private readonly sortDirSig = signal<SortDir>('desc');

  readonly computedInvoices = computed(() => computeAll(this.invoicesSig()));
  readonly dashboardAggregates = computed(() => buildDashboardAggregates(this.computedInvoices()));
  readonly gridRows = computed(() =>
    buildGridRows(this.computedInvoices(), this.searchSig(), this.statusFilterSig(), this.sortKeySig(), this.sortDirSig()),
  );

  readonly loadError = this.loadErrorSig.asReadonly();

  readonly pool = this.poolSig.asReadonly();
  readonly queuedFiles = this.queuedFilesSig.asReadonly();
  readonly processing = this.processingSig.asReadonly();
  readonly batchResults = this.batchResultsSig.asReadonly();
  readonly processDisabled = computed(
    () => this.processingSig() || (this.queuedFilesSig().length === 0 && this.poolSig().length === 0),
  );
  readonly processLabel = computed(() => {
    if (this.processingSig()) return 'Processing…';
    if (this.poolSig().length === 0 && this.queuedFilesSig().length === 0) return 'No more sample invoices';
    return 'Process invoices';
  });

  readonly editing = this.editingSig.asReadonly();
  readonly editDraft = this.editDraftSig.asReadonly();
  readonly saving = this.savingSig.asReadonly();
  readonly saveError = this.saveErrorSig.asReadonly();
  readonly search = this.searchSig.asReadonly();
  readonly statusFilter = this.statusFilterSig.asReadonly();
  readonly sortKey = this.sortKeySig.asReadonly();
  readonly sortDir = this.sortDirSig.asReadonly();

  // Called once from app.config.ts's provideAppInitializer. Swallows its
  // own errors and resolves to an empty list rather than propagating, so a
  // down API boots an empty-but-usable app instead of a permanently blank
  // screen.
  loadInvoices(): Observable<StoredInvoiceDto[]> {
    return this.invoiceApi.getAll().pipe(
      tap((dtos) => {
        this.invoicesSig.set(dtos.map(toInvoice));
        this.loadErrorSig.set(null);
      }),
      catchError((err: HttpErrorResponse) => {
        this.loadErrorSig.set(extractErrorMessage(err));
        return of([]);
      }),
    );
  }

  setSearch(value: string): void {
    this.searchSig.set(value);
  }

  setStatusFilter(value: 'all' | InvoiceStatus): void {
    this.statusFilterSig.set(value);
  }

  setSort(key: SortKey): void {
    if (this.sortKeySig() === key) {
      this.sortDirSig.update((d) => (d === 'asc' ? 'desc' : 'asc'));
    } else {
      this.sortKeySig.set(key);
      this.sortDirSig.set('asc');
    }
  }

  setQueuedFiles(files: File[]): void {
    this.queuedFilesSig.set(files);
    this.batchResultsSig.set(null);
  }

  invoiceExists(id: string): boolean {
    return this.invoicesSig().some((inv) => inv.id === id);
  }

  // Dispatches to whichever source has real work to do: real files queued
  // (extract each, then classify it) take priority over the sample pool,
  // matching the confirmed design ("real upload is additive, not a
  // replacement" — both paths stay available, this just decides which one
  // a given click uses).
  processInvoices(): void {
    if (this.processingSig()) return;
    const files = this.queuedFilesSig();
    if (files.length > 0) {
      this.processUploadedFiles(files);
    } else {
      this.processPoolInvoices();
    }
  }

  // Per file: extract -> classify, chained with switchMap. A file's
  // extraction failure is caught here (there's no UnclassifiedInvoice yet
  // to attach the error to, so failedExtractionPlaceholder stands in for
  // display); a file's classification failure is caught inside the chain,
  // same per-item isolation the pool path already has. forkJoin combines
  // every file's outcome so one bad file never sinks the batch.
  private processUploadedFiles(files: File[]): void {
    this.processingSig.set(true);

    const calls = files.map((file) =>
      this.extractApi.extract(file).pipe(
        switchMap((extracted) => {
          const raw = toUnclassifiedInvoice(extracted);
          return this.classifyApi.classify(toClassifyRequest(raw)).pipe(
            map((response): BatchResult => ({ outcome: 'success', raw, invoice: mergeClassification(raw, response) })),
            catchError((err: HttpErrorResponse) =>
              of<BatchResult>({ outcome: 'error', raw, message: extractErrorMessage(err) }),
            ),
          );
        }),
        catchError((err: HttpErrorResponse) =>
          of<BatchResult>({ outcome: 'error', raw: failedExtractionPlaceholder(file), message: extractErrorMessage(err) }),
        ),
      ),
    );

    forkJoin(calls).subscribe((results) => {
      const succeeded = results.filter(isSuccess);
      this.invoicesSig.update((list) => [...list, ...succeeded.map((r) => r.invoice)]);
      this.batchResultsSig.set(results);
      this.queuedFilesSig.set([]);
      this.processingSig.set(false);
    });
  }

  // Unchanged behavior from before real upload existed: one pool invoice
  // per click. The old batch-size expression, clamp(queuedFiles.length, 1,
  // pool.length), depended on the queued-file *count* as a proxy signal;
  // this method is now only ever reached with an empty queue (non-empty
  // queues route to processUploadedFiles instead), so that expression
  // always evaluated to 1 here regardless, simplified accordingly.
  private processPoolInvoices(): void {
    const pool = this.poolSig();
    if (pool.length === 0) return;

    const taken = pool.slice(0, 1);
    this.processingSig.set(true);

    const calls = taken.map((raw) =>
      this.classifyApi.classify(toClassifyRequest(raw)).pipe(
        map((response): BatchResult => ({ outcome: 'success', raw, invoice: mergeClassification(raw, response) })),
        catchError((err: HttpErrorResponse) =>
          of<BatchResult>({ outcome: 'error', raw, message: extractErrorMessage(err) }),
        ),
      ),
    );

    forkJoin(calls).subscribe((results) => {
      const succeeded = results.filter(isSuccess);
      const succeededIds = new Set(succeeded.map((r) => r.raw.invoiceId));
      this.invoicesSig.update((list) => [...list, ...succeeded.map((r) => r.invoice)]);
      this.poolSig.update((p) => p.filter((inv) => !succeededIds.has(inv.invoiceId)));
      this.batchResultsSig.set(results);
      this.queuedFilesSig.set([]);
      this.processingSig.set(false);
    });
  }

  startEdit(invoiceId: string, lineIndex: number, current: Classification): void {
    this.editingSig.set({ invoiceId, lineIndex });
    this.editDraftSig.set({ classification: current, reason: '' });
    this.saveErrorSig.set(null);
  }

  setEditClassification(classification: Classification): void {
    this.editDraftSig.update((d) => ({ ...d, classification }));
  }

  setEditReason(reason: string): void {
    this.editDraftSig.update((d) => ({ ...d, reason }));
  }

  cancelEdit(): void {
    this.editingSig.set(null);
    this.saveErrorSig.set(null);
  }

  // Persists the override via PATCH rather than mutating local state
  // directly, so a correction survives a reload. editingSig is deliberately
  // left open on failure so the user's draft survives a retry.
  saveEdit(): void {
    const editing = this.editingSig();
    const draft = this.editDraftSig();
    if (!editing || !draft.reason.trim() || this.savingSig()) return;

    const invoice = this.invoicesSig().find((inv) => inv.id === editing.invoiceId);
    const line = invoice?.lines[editing.lineIndex];
    if (!invoice || !line) return;

    const newLabel: ApiLabel = draft.classification === 'construction' ? 'Construction' : 'Services';
    this.savingSig.set(true);
    this.saveErrorSig.set(null);

    // invoice.ref (not invoice.id) is the canonical uppercase invoice id
    // the API expects, matching CisCompliance.Core's InvoiceId convention.
    this.invoiceApi.overrideLine(invoice.ref, line.lineId, newLabel, draft.reason.trim()).subscribe({
      next: (dto) => {
        const updated = toInvoice(dto);
        this.invoicesSig.update((list) => list.map((inv) => (inv.id === updated.id ? updated : inv)));
        this.savingSig.set(false);
        this.editingSig.set(null);
      },
      error: (err: HttpErrorResponse) => {
        this.savingSig.set(false);
        this.saveErrorSig.set(extractErrorMessage(err));
      },
    });
  }
}
