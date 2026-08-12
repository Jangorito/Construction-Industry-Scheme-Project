import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';

import { InvoiceStore } from '../../core/state/invoice-store';
import { computeInvoice } from '../../core/utils/invoice-calculations';
import { GbpCurrencyPipe } from '../../shared/pipes/gbp-currency.pipe';

@Component({
  selector: 'app-upload',
  standalone: true,
  imports: [RouterLink, GbpCurrencyPipe],
  templateUrl: './upload.component.html',
  styleUrl: './upload.component.css',
})
export class UploadComponent {
  protected readonly store = inject(InvoiceStore);

  // Cosmetic only, matching the mockup: captures file names for display,
  // never reads file content. The pool invoices (not these files) are what
  // actually get sent to the real API, see InvoiceStore.processInvoices().
  protected onFilesSelected(event: Event): void {
    const files = (event.target as HTMLInputElement).files;
    const names = files ? Array.from(files).map((f) => f.name) : [];
    this.store.setQueuedFiles(names);
  }

  // computeInvoice() needs a full Invoice, batch results only carry the
  // narrower shape, this recomputes totals/status for display in the
  // processed-batch summary panel.
  protected summaryFor(invoice: Parameters<typeof computeInvoice>[0]) {
    return computeInvoice(invoice);
  }
}
