import { HttpErrorResponse } from '@angular/common/http';

import { ClassifyRequest, ClassifyResponse, ProblemDetails, RuleCitationDto } from '../models/classify-api.model';
import { Classification, Confidence, Invoice } from '../models/invoice.model';
import { UnclassifiedInvoice } from '../models/unclassified-invoice.model';

export function toClassifyRequest(raw: UnclassifiedInvoice): ClassifyRequest {
  return {
    invoiceId: raw.invoiceId,
    supplier: raw.supplier,
    customer: raw.customer,
    date: raw.date,
    lineItems: raw.lineItems.map((li) => ({ lineId: li.lineId, description: li.description, amountGbp: li.amountGbp })),
  };
}

// Only resolved citations are shown, an unresolved one has no source to
// display. Structurally this should be unreachable (the API's response
// schema enum only allows real rule ids), but the fallback exists so a line
// never renders a blank citation if that constraint is ever relaxed.
export function joinCitations(citations: RuleCitationDto[]): string {
  const resolved = citations.filter((c) => c.resolved);
  if (resolved.length === 0) return 'No verified citations';
  return resolved.map((c) => `${c.ruleId} (${c.sourceName})`).join(', ');
}

// Merges the API's response back onto the original request's line items by
// lineId, since the response itself only carries lineId + the classification
// fields, not description/amount. Throws if the model somehow didn't return
// every requested line, the caller (InvoiceStore) turns that into a
// per-invoice error rather than rendering a broken/incomplete invoice.
export function mergeClassification(raw: UnclassifiedInvoice, response: ClassifyResponse): Invoice {
  const byLineId = new Map(response.lineItems.map((li) => [li.lineId, li]));

  const lines = raw.lineItems.map((li) => {
    const predicted = byLineId.get(li.lineId);
    if (!predicted) {
      throw new Error(`Classification response is missing line item '${li.lineId}'.`);
    }
    return {
      // Not cosmetic: this is what lets a just-uploaded invoice be
      // overridden before any GET /api/invoices refetch, the id sent here
      // is exactly what ClassifyController's SaveAsync persisted.
      lineId: li.lineId,
      description: li.description,
      amount: li.amountGbp,
      classification: predicted.label.toLowerCase() as Classification,
      confidence: predicted.confidence.toLowerCase() as Confidence,
      citation: joinCitations(predicted.citations),
      reasoning: predicted.reasoning,
      overridden: false,
      overrideReason: '',
    };
  });

  return { id: raw.invoiceId.toLowerCase(), ref: raw.ref, supplier: raw.supplier, date: raw.date, lines };
}

export function extractErrorMessage(err: HttpErrorResponse): string {
  const problem = err.error as ProblemDetails | undefined;
  return problem?.detail ?? problem?.title ?? err.statusText ?? 'Classification failed.';
}
