import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';

import { InvoiceStore } from '../../core/state/invoice-store';
import { InvoiceStatus } from '../../core/models/invoice.model';
import { SortKey } from '../../core/utils/invoice-calculations';
import { GbpCurrencyPipe } from '../../shared/pipes/gbp-currency.pipe';
import { UkDatePipe } from '../../shared/pipes/uk-date.pipe';

interface GridColumn {
  key: SortKey;
  label: string;
}

const COLUMNS: GridColumn[] = [
  { key: 'supplier', label: 'Supplier' },
  { key: 'ref', label: 'Reference' },
  { key: 'date', label: 'Invoice date' },
  { key: 'total', label: 'Total value' },
  { key: 'construction', label: 'Construction' },
  { key: 'services', label: 'Services' },
];

const STATUS_FILTERS: ('all' | InvoiceStatus)[] = ['all', 'Needs review', 'Flagged', 'Reviewed'];

@Component({
  selector: 'app-invoice-grid',
  standalone: true,
  imports: [RouterLink, GbpCurrencyPipe, UkDatePipe],
  templateUrl: './invoice-grid.component.html',
  styleUrl: './invoice-grid.component.css',
})
export class InvoiceGridComponent {
  protected readonly store = inject(InvoiceStore);
  protected readonly columns = COLUMNS;
  protected readonly statusFilters = STATUS_FILTERS;

  protected onSearchInput(event: Event): void {
    this.store.setSearch((event.target as HTMLInputElement).value);
  }

  // '↑'/'↓' for the active sort column, blank otherwise, matching the
  // mockup's arrow indicator.
  protected arrowFor(key: SortKey): string {
    if (this.store.sortKey() !== key) return '';
    return this.store.sortDir() === 'asc' ? '↑' : '↓';
  }
}
