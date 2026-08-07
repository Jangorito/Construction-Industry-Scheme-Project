namespace CisCompliance.Core.Models;

public sealed record InvoiceClassificationResult(string InvoiceId, IReadOnlyList<LineItemClassification> LineItems);
