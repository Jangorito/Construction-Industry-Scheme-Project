namespace CisCompliance.Core.Models;

// Summary:
// Represents a single line item in an invoice, as submitted for classification.
public sealed record InvoiceLineItemRequest(string LineId, string Description, decimal AmountGbp);
