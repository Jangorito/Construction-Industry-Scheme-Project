import { ClassifyRequest } from './classify-api.model';

// The /api/extract response IS an InvoiceClassificationRequest on the
// server (ExtractController returns the exact type ClassifyController
// accepts) — a type alias, not a second definition, so the two can never
// drift out of sync the way two independently hand-written interfaces could.
export type ExtractResponse = ClassifyRequest;
