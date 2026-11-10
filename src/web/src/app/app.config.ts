import { ApplicationConfig, inject, provideAppInitializer, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { routes } from './app.routes';
import { InvoiceStore } from './core/state/invoice-store';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes, withComponentInputBinding()),
    provideHttpClient(),
    // Blocks the very first router navigation on one GET /api/invoices
    // call, so invoicesSig is guaranteed populated (or definitively
    // failed-and-empty) before any route or guard ever runs, covering all
    // 4 feature routes without duplicating loading logic into each one.
    provideAppInitializer(() => {
      const store = inject(InvoiceStore);
      return firstValueFrom(store.loadInvoices());
    })
  ]
};
