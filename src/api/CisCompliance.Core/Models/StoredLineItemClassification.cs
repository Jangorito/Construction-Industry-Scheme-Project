namespace CisCompliance.Core.Models;

// The full, persisted shape of one classified line item, distinct from
// LineItemClassification (the classify-response-only shape, which has no
// description/amount since the client already has those). PredictedLabel is
// written once and never changes, it's the audit record of what the AI
// actually said. EffectiveLabel starts equal to it and only moves if a human
// overrides the line, so both the original answer and any correction stay
// visible, neither gets silently overwritten.
public sealed record StoredLineItemClassification(
    string LineId,
    string Description,
    decimal AmountGbp,
    ClassificationLabel PredictedLabel,
    ConfidenceLevel PredictedConfidence,
    string Reasoning,
    IReadOnlyList<RuleCitation> Citations,
    ClassificationLabel EffectiveLabel,
    bool IsOverridden,
    string? OverrideReason,
    DateTimeOffset? OverriddenAtUtc);
