// The "not yet processed" shape used by the upload pool: real invoice line
// items with no classification/confidence/citation/reasoning attached yet,
// since that only exists once the real API has classified them.
export interface UnclassifiedLineItem {
  lineId: string;
  description: string;
  amountGbp: number;
}

export interface UnclassifiedInvoice {
  invoiceId: string;
  ref: string;
  supplier: string;
  customer: string;
  date: string;
  lineItems: UnclassifiedLineItem[];
}
