using CisCompliance.Core.Models;

namespace CisCompliance.Core.Abstractions;

// GetByIdAsync/OverrideLineAsync return null (not an exception) when an
// invoice or line doesn't exist, so Core stays free of any HTTP-status
// concept. InvoicesController is what turns a null into a 404.
public interface IInvoiceRepository
{
    Task SaveAsync(
        InvoiceClassificationRequest request,
        InvoiceClassificationResult result,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<StoredInvoice>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<StoredInvoice?> GetByIdAsync(string invoiceId, CancellationToken cancellationToken = default);

    Task<StoredInvoice?> OverrideLineAsync(
        string invoiceId,
        string lineId,
        ClassificationLabel newLabel,
        string reason,
        CancellationToken cancellationToken = default);
}
