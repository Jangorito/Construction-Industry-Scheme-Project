import { ApiConfidence, ApiLabel, RuleCitationDto } from './classify-api.model';

// Mirrors CisCompliance.Core.Models.StoredInvoice / StoredLineItemClassification.
export interface StoredLineItemDto {
  lineId: string;
  description: string;
  amountGbp: number;
  predictedLabel: ApiLabel;
  predictedConfidence: ApiConfidence;
  reasoning: string;
  citations: RuleCitationDto[];
  effectiveLabel: ApiLabel;
  isOverridden: boolean;
  overrideReason: string | null;
  overriddenAtUtc: string | null;
}

export interface StoredInvoiceDto {
  invoiceId: string;
  supplier: string;
  customer: string;
  date: string;
  createdAtUtc: string;
  lineItems: StoredLineItemDto[];
}
