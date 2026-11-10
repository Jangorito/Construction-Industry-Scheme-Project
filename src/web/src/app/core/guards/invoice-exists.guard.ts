import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { InvoiceStore } from '../state/invoice-store';

// The mockup left a detail route for an unknown invoice id blank (all 4
// sc-if blocks failed their guard). Redirecting to the grid instead is a
// deliberate fix, not a reproduction of that edge case. Safe to check
// synchronously here even though invoice data now comes from an async
// fetch: app.config.ts's provideAppInitializer awaits that fetch once
// before the router activates any route, so by the time this guard ever
// runs the store is already loaded (or definitively empty), never mid-load.
export const invoiceExistsGuard: CanActivateFn = (route) => {
  const store = inject(InvoiceStore);
  const router = inject(Router);
  const id = route.paramMap.get('id');
  return id && store.invoiceExists(id) ? true : router.parseUrl('/invoices');
};
