namespace CisCompliance.Core.Models;

public sealed record LineItemClassification(
    string LineId,
    ClassificationLabel Label,
    ConfidenceLevel Confidence,
    string Reasoning,
    IReadOnlyList<RuleCitation> Citations);
