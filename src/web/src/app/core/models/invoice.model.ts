// Display-ready invoice shape, ported from the mockup's data model
// (POC/Mock Up/CIS Invoice Compliance Tool). overridden/overrideReason
// are session-only, never sent to any backend, there's no persistence
// endpoint for them yet.
export type Classification = 'construction' | 'services';
export type Confidence = 'high' | 'low';
export type InvoiceStatus = 'Reviewed' | 'Flagged' | 'Needs review';

export interface InvoiceLine {
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
