import { Classification, Confidence, Invoice } from '../models/invoice.model';
import { StoredInvoiceDto } from '../models/stored-invoice.model';
import { joinCitations } from './classification-mapper';

// Mirrors the id/ref lowercasing convention mergeClassification() already
// uses, so DB-loaded and freshly-live-classified invoices produce
// identically-shaped id/ref values.
export function toInvoice(dto: StoredInvoiceDto): Invoice {
  return {
    id: dto.invoiceId.toLowerCase(),
    ref: dto.invoiceId,
    supplier: dto.supplier,
    date: dto.date,
    lines: dto.lineItems.map((li) => ({
      lineId: li.lineId,
      description: li.description,
      amount: li.amountGbp,
      classification: li.effectiveLabel.toLowerCase() as Classification,
      confidence: li.predictedConfidence.toLowerCase() as Confidence,
      citation: joinCitations(li.citations),
      reasoning: li.reasoning,
      overridden: li.isOverridden,
      overrideReason: li.overrideReason ?? '',
    })),
  };
}
