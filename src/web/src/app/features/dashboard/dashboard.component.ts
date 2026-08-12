import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';

import { InvoiceStore } from '../../core/state/invoice-store';
import { GbpCurrencyPipe } from '../../shared/pipes/gbp-currency.pipe';
import { UkDatePipe } from '../../shared/pipes/uk-date.pipe';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink, GbpCurrencyPipe, UkDatePipe],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css',
})
export class DashboardComponent {
  protected readonly store = inject(InvoiceStore);

  protected donutGradient(pct: number): string {
    return `conic-gradient(var(--color-accent-600) 0% ${pct}%, var(--color-neutral-300) ${pct}% 100%)`;
  }
}
