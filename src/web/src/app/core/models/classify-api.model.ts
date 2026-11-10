// Mirrors CisCompliance.Core.Models (see src/api/CisCompliance.Core/Models/)
// property for property, camelCase to match System.Text.Json's default wire
// format. Keeping this a direct mirror means a backend contract drift shows
// up as a TypeScript compile error here, not a silent runtime mismatch.

export interface ClassifyLineItemRequest {
  lineId: string;
  description: string;
  amountGbp: number;
}

export interface ClassifyRequest {
  invoiceId: string;
  supplier: string;
  customer: string;
  date: string;
  lineItems: ClassifyLineItemRequest[];
}

export type ApiLabel = 'Construction' | 'Services';
export type ApiConfidence = 'High' | 'Low';

export interface RuleCitationDto {
  ruleId: string;
  resolved: boolean;
  sourceName: string | null;
  sourceTitle: string | null;
  sourceUrl: string | null;
  verified: string | null;
}

export interface LineItemClassificationDto {
  lineId: string;
  label: ApiLabel;
  confidence: ApiConfidence;
  reasoning: string;
  citations: RuleCitationDto[];
}

export interface ClassifyResponse {
  invoiceId: string;
  lineItems: LineItemClassificationDto[];
}

// Shape of the RFC7807 Problem body ClassifyController returns on a 502.
export interface ProblemDetails {
  type?: string;
  title?: string;
  detail?: string;
  status?: number;
}
