namespace CisCompliance.Core.Models;

// Deliberately has no ground truth field. That was a generate_invoices.py /
// classify.py POC-only concept for scoring against a known answer; a real
// invoice submitted for classification has no known answer to begin with.
public sealed record InvoiceClassificationRequest(
    string InvoiceId,
    string Supplier,
    string Customer,
    DateOnly Date,
    IReadOnlyList<InvoiceLineItemRequest> LineItems);
