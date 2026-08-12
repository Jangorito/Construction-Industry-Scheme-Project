import { Routes } from '@angular/router';

import { invoiceExistsGuard } from './core/guards/invoice-exists.guard';

// Real routes, a deliberate improvement over the mockup, which had no URL
// state at all (a single state.view field switched between 4 sc-if blocks).
// There's no URL state to preserve from the original, so this is free.
export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  {
    path: 'dashboard',
    loadComponent: () => import('./features/dashboard/dashboard.component').then((m) => m.DashboardComponent),
  },
  {
    path: 'invoices',
    loadComponent: () => import('./features/invoice-grid/invoice-grid.component').then((m) => m.InvoiceGridComponent),
  },
  {
    path: 'invoices/:id',
    canActivate: [invoiceExistsGuard],
    loadComponent: () =>
      import('./features/invoice-detail/invoice-detail.component').then((m) => m.InvoiceDetailComponent),
  },
  {
    path: 'upload',
    loadComponent: () => import('./features/upload/upload.component').then((m) => m.UploadComponent),
  },
  { path: '**', redirectTo: 'dashboard' },
];
