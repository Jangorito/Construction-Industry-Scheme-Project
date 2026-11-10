namespace CisCompliance.Core.Models;


// Summary:
// Represents a whole invoice submitted(/inputted) for classification, including the 
//  invoice's metadata and all its line items as InvoiceLineItemRequest objects.
public sealed record InvoiceClassificationRequest(
    string InvoiceId,
    string Supplier,
    string Customer,
    DateOnly Date,
    IReadOnlyList<InvoiceLineItemRequest> LineItems);
