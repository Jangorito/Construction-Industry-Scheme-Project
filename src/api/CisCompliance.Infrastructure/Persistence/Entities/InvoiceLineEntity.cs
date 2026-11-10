using CisCompliance.Core.Models;

namespace CisCompliance.Infrastructure.Persistence.Entities;

// PredictedLabel/PredictedConfidence are written once at save time and never
// touched again, that's the audit record of what the AI actually said.
// EffectiveLabel starts equal to PredictedLabel and only OverrideLineAsync
// changes it, so a human correction never overwrites the original answer.
public sealed class InvoiceLineEntity
{
    public int Id { get; set; }
    public string InvoiceId { get; set; } = string.Empty;
    public InvoiceEntity Invoice { get; set; } = null!;
    public string LineId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal AmountGbp { get; set; }
    public ClassificationLabel PredictedLabel { get; set; }
    public ConfidenceLevel PredictedConfidence { get; set; }
    public string Reasoning { get; set; } = string.Empty;
    public ClassificationLabel EffectiveLabel { get; set; }
    public bool IsOverridden { get; set; }
    public string? OverrideReason { get; set; }
    public DateTimeOffset? OverriddenAtUtc { get; set; }
    public List<RuleCitationEntity> Citations { get; set; } = new();
}
