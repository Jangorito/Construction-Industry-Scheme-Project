using CisCompliance.Core.Models;

namespace CisCompliance.Core.Abstractions;

public interface IClassificationService
{
    Task<InvoiceClassificationResult> ClassifyAsync(
        InvoiceClassificationRequest request, CancellationToken cancellationToken = default);
}
