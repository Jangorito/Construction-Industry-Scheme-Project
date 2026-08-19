// Display-ready invoice shape, ported from the mockup's data model
// (POC/Mock Up/CIS Invoice Compliance Tool).
export type Classification = 'construction' | 'services';
export type Confidence = 'high' | 'low';
export type InvoiceStatus = 'Reviewed' | 'Flagged' | 'Needs review';

export interface InvoiceLine {
  // Needed to address a specific line for the override PATCH endpoint,
  // lines were previously only addressed by array index.
  lineId: string;
  description: string;
  amount: number;
  classification: Classification;
  confidence: Confidence;
  citation: string;
  reasoning: string;
  overridden: boolean;
  overrideReason: string;
}

export interface Invoice {
  id: string;
  ref: string;
  supplier: string;
  date: string;
  lines: InvoiceLine[];
}
