namespace CisCompliance.Core.Models;

// Summary:
// Represents the shape of a request to override a line item's classification label.
// InvoicesController rejects empty or blank reason
public sealed record OverrideLineRequest(ClassificationLabel NewLabel, string Reason);
