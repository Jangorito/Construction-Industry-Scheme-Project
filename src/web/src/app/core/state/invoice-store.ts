import { HttpErrorResponse } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, catchError, forkJoin, map, of, tap } from 'rxjs';

import { UPLOAD_POOL } from '../data/upload-pool';
import { ApiLabel } from '../models/classify-api.model';
import { Classification, Invoice, InvoiceStatus } from '../models/invoice.model';
import { StoredInvoiceDto } from '../models/stored-invoice.model';
import { UnclassifiedInvoice } from '../models/unclassified-invoice.model';
import { ClassifyApiService } from '../services/classify-api.service';
import { InvoiceApiService } from '../services/invoice-api.service';
import { extractErrorMessage, mergeClassification, toClassifyRequest } from '../utils/classification-mapper';
import { SortDir, SortKey, buildDashboardAggregates, buildGridRows, clamp, computeAll } from '../utils/invoice-calculations';
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
  private readonly invoiceApi = inject(InvoiceApiService);

  // Starts empty, populated by loadInvoices(). Never observed empty by a
  // routed component: app.config.ts's provideAppInitializer awaits
  // loadInvoices() once before the router activates any route.
  private readonly invoicesSig = signal<Invoice[]>([]);
  private readonly loadErrorSig = signal<string | null>(null);

  private readonly poolSig = signal<UnclassifiedInvoice[]>(UPLOAD_POOL);
  private readonly queuedFilesSig = signal<string[]>([]);
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

  setQueuedFiles(names: string[]): void {
    this.queuedFilesSig.set(names);
    this.batchResultsSig.set(null);
  }

  invoiceExists(id: string): boolean {
    return this.invoicesSig().some((inv) => inv.id === id);
  }

  // Takes clamp(queuedFiles, 1, pool.length) invoices off the pool and
  // classifies each with a real API call. Each call's failure is caught
  // independently so one bad invoice never sinks the whole batch, failures
  // stay in the pool for a retry on the next click, successes get appended
  // to the invoice list and removed from the pool.
  processInvoices(): void {
    const pool = this.poolSig();
    if (this.processingSig() || pool.length === 0) return;

    const count = clamp(this.queuedFilesSig().length, 1, pool.length);
    const taken = pool.slice(0, count);
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
