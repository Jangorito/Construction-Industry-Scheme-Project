using CisCompliance.Core.Models;

namespace CisCompliance.Core.Abstractions;

// Returns InvoiceClassificationRequest directly, the same type
// ClassifyController already accepts, so extraction and classification
// chain with zero mapping code at the Api layer.
public interface IInvoiceExtractionService
{
    Task<InvoiceClassificationRequest> ExtractAsync(
        byte[] fileBytes, string mediaType, string fileName, CancellationToken cancellationToken = default);
}
