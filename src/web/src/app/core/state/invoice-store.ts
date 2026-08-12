import { HttpErrorResponse } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { catchError, forkJoin, map, of } from 'rxjs';

import { SEED_INVOICES, SeedInvoice } from '../data/seed-invoices';
import { UPLOAD_POOL } from '../data/upload-pool';
import { Classification, Invoice, InvoiceStatus } from '../models/invoice.model';
import { UnclassifiedInvoice } from '../models/unclassified-invoice.model';
import { ClassifyApiService } from '../services/classify-api.service';
import { extractErrorMessage, mergeClassification, toClassifyRequest } from '../utils/classification-mapper';
import { SortDir, SortKey, buildDashboardAggregates, buildGridRows, clamp, computeAll } from '../utils/invoice-calculations';

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

function normalizeSeed(seed: SeedInvoice[]): Invoice[] {
  return seed.map((inv) => ({
    ...inv,
    lines: inv.lines.map((l) => ({ ...l, overridden: false, overrideReason: '' })),
  }));
}

// Single source of truth for the whole app: the invoice list, the upload
// pool/queue, and UI state (search/filter/sort/editing). Mirrors the
// mockup's "flat state, derived values recomputed fresh" pattern using
// signals instead of a custom re-render loop.
@Injectable({ providedIn: 'root' })
export class InvoiceStore {
  private readonly classifyApi = inject(ClassifyApiService);

  private readonly invoicesSig = signal<Invoice[]>(normalizeSeed(SEED_INVOICES));
  private readonly poolSig = signal<UnclassifiedInvoice[]>(UPLOAD_POOL);
  private readonly queuedFilesSig = signal<string[]>([]);
  private readonly processingSig = signal(false);
  private readonly batchResultsSig = signal<BatchResult[] | null>(null);

  private readonly editingSig = signal<EditingTarget | null>(null);
  private readonly editDraftSig = signal<EditDraft>({ classification: 'construction', reason: '' });

  private readonly searchSig = signal('');
  private readonly statusFilterSig = signal<'all' | InvoiceStatus>('all');
  private readonly sortKeySig = signal<SortKey>('date');
  private readonly sortDirSig = signal<SortDir>('desc');

  readonly computedInvoices = computed(() => computeAll(this.invoicesSig()));
  readonly dashboardAggregates = computed(() => buildDashboardAggregates(this.computedInvoices()));
  readonly gridRows = computed(() =>
    buildGridRows(this.computedInvoices(), this.searchSig(), this.statusFilterSig(), this.sortKeySig(), this.sortDirSig()),
  );

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
  readonly search = this.searchSig.asReadonly();
  readonly statusFilter = this.statusFilterSig.asReadonly();
  readonly sortKey = this.sortKeySig.asReadonly();
  readonly sortDir = this.sortDirSig.asReadonly();

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
  // stay in the pool for a retry on the next click, successes are appended
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
  }

  setEditClassification(classification: Classification): void {
    this.editDraftSig.update((d) => ({ ...d, classification }));
  }

  setEditReason(reason: string): void {
    this.editDraftSig.update((d) => ({ ...d, reason }));
  }

  cancelEdit(): void {
    this.editingSig.set(null);
  }

  saveEdit(): void {
    const editing = this.editingSig();
    const draft = this.editDraftSig();
    if (!editing) return;
    if (!draft.reason.trim()) return;

    this.invoicesSig.update((list) =>
      list.map((inv) =>
        inv.id !== editing.invoiceId
          ? inv
          : {
              ...inv,
              lines: inv.lines.map((l, i) =>
                i !== editing.lineIndex
                  ? l
                  : { ...l, classification: draft.classification, overridden: true, overrideReason: draft.reason.trim() },
              ),
            },
      ),
    );
    this.editingSig.set(null);
  }
}
