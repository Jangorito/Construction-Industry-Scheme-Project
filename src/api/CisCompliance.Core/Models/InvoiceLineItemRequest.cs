namespace CisCompliance.Core.Models;

// AmountGbp is decimal, not double, because this is currency: decimal avoids
// the binary floating point rounding that would otherwise creep into totals.
public sealed record InvoiceLineItemRequest(string LineId, string Description, decimal AmountGbp);
