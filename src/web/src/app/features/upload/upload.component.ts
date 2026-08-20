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

  // Real files now, not just names: if any are selected, processInvoices()
  // sends each through /api/extract then /api/classify. With no files
  // selected, the existing sample pool is used instead, unchanged.
  protected onFilesSelected(event: Event): void {
    const files = (event.target as HTMLInputElement).files;
    this.store.setQueuedFiles(files ? Array.from(files) : []);
  }

  // computeInvoice() needs a full Invoice, batch results only carry the
  // narrower shape, this recomputes totals/status for display in the
  // processed-batch summary panel.
  protected summaryFor(invoice: Parameters<typeof computeInvoice>[0]) {
    return computeInvoice(invoice);
  }
}
