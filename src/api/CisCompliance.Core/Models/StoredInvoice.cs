namespace CisCompliance.Core.Models;

// What IInvoiceRepository hands back for GET /api/invoices and
// GET /api/invoices/{id}: everything ClassifyResponse has, plus the fields
// only the repository knows (description/amount, since the client already
// had those when it sent the classify request, and override/audit state).
public sealed record StoredInvoice(
    string InvoiceId,
    string Supplier,
    string Customer,
    DateOnly Date,
    DateTimeOffset CreatedAtUtc,
    IReadOnlyList<StoredLineItemClassification> LineItems);
