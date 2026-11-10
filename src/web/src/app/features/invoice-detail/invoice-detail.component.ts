import { Component, OnDestroy, computed, inject, input } from '@angular/core';

import { InvoiceStore } from '../../core/state/invoice-store';
import { Classification } from '../../core/models/invoice.model';
import { GbpCurrencyPipe } from '../../shared/pipes/gbp-currency.pipe';
import { UkDatePipe } from '../../shared/pipes/uk-date.pipe';

@Component({
  selector: 'app-invoice-detail',
  standalone: true,
  imports: [GbpCurrencyPipe, UkDatePipe],
  templateUrl: './invoice-detail.component.html',
  styleUrl: './invoice-detail.component.css',
})
export class InvoiceDetailComponent implements OnDestroy {
  protected readonly store = inject(InvoiceStore);

  // Bound automatically by withComponentInputBinding() from the :id route
  // param. invoiceExistsGuard already guarantees this resolves to a real
  // invoice before this component is ever activated.
  readonly id = input.required<string>();

  protected readonly entry = computed(() => this.store.computedInvoices().find((c) => c.inv.id === this.id()));

  protected isEditing(lineIndex: number): boolean {
    const editing = this.store.editing();
    return !!editing && editing.invoiceId === this.id() && editing.lineIndex === lineIndex;
  }

  protected startEdit(lineIndex: number, current: Classification): void {
    this.store.startEdit(this.id(), lineIndex, current);
  }

  protected classLabel(c: Classification): string {
    return c === 'construction' ? 'Construction' : 'Services';
  }

  protected classTagClass(c: Classification): string {
    return c === 'construction' ? 'tag tag-accent-2' : 'tag tag-neutral';
  }

  protected confidenceLabel(confidence: string): string {
    return confidence === 'high' ? 'High confidence' : 'Low confidence';
  }

  protected confidenceTagClass(confidence: string): string {
    return confidence === 'high' ? 'tag tag-neutral' : 'tag tag-accent';
  }

  protected onReasonInput(event: Event): void {
    this.store.setEditReason((event.target as HTMLTextAreaElement).value);
  }

  // Resets stale edit state so leaving mid-edit never bleeds into whatever
  // invoice gets viewed next.
  ngOnDestroy(): void {
    this.store.cancelEdit();
  }
}
