import { UnclassifiedInvoice } from '../models/unclassified-invoice.model';

// Real, not-yet-classified invoices from the POC's validated test set
// (POC/data/invoices/INV-2026-0009.json, INV-2026-0010.json), stripped of
// their _ground_truth blocks (that's POC-only scoring data, never sent
// anywhere). The upload flow POSTs these to the real /api/classify
// endpoint one at a time, this is not a fake simulation.
export const UPLOAD_POOL: UnclassifiedInvoice[] = [
  {
    invoiceId: 'INV-2026-0009', ref: 'INV-2026-0009', supplier: 'Camden Bridge Construction Group Ltd', customer: 'Oakfield Property Group', date: '2026-05-29',
    lineItems: [
      { lineId: 'INV-2026-0009-L1', description: 'Supply and fit carpet tiles as part of wider office refurbishment contract', amountGbp: 4377.46 },
      { lineId: 'INV-2026-0009-L2', description: 'Supply and fit carpet tiles, open-plan office', amountGbp: 3285.26 },
      { lineId: 'INV-2026-0009-L3', description: 'Consultancy, remedial works', amountGbp: 3570.20 },
      { lineId: 'INV-2026-0009-L4', description: 'Supply and erect security fencing around site compound perimeter', amountGbp: 5216.68 },
      { lineId: 'INV-2026-0009-L5', description: 'Installation and repair of glazing, warehouse windows', amountGbp: 3543.31 },
      { lineId: 'INV-2026-0009-L6', description: 'Subcontractor accommodation costs, 6 weeks on-site', amountGbp: 3470.87 },
    ],
  },
  {
    invoiceId: 'INV-2026-0010', ref: 'INV-2026-0010', supplier: 'Oakfield General Contracting Ltd', customer: 'Oakfield Property Group', date: '2026-06-13',
    lineItems: [
      { lineId: 'INV-2026-0010-L1', description: 'Subcontractor accommodation costs, 3 months on-site', amountGbp: 2653.35 },
      { lineId: 'INV-2026-0010-L2', description: 'Installation of louvered security shutters, shopfront', amountGbp: 4175.05 },
      { lineId: 'INV-2026-0010-L3', description: 'Tree felling and landscaping in the ordinary course of estate management (no development project)', amountGbp: 2622.21 },
      { lineId: 'INV-2026-0010-L4', description: 'Supply and install retractable awning, shopfront', amountGbp: 2444.48 },
      { lineId: 'INV-2026-0010-L5', description: 'Site clearance and earth-moving ahead of construction', amountGbp: 5065.05 },
    ],
  },
];
