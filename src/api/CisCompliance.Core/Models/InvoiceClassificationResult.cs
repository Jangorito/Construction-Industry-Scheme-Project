namespace CisCompliance.Core.Models;
// Summary:
// Represents model's output of a classified invoice
// Created by the ClassificationService.
public sealed record InvoiceClassificationResult(string InvoiceId, IReadOnlyList<LineItemClassification> LineItems);
