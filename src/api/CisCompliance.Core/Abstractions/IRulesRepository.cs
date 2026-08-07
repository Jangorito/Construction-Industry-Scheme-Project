using CisCompliance.Core.Models;

namespace CisCompliance.Core.Abstractions;

public interface IRulesRepository
{
    Task<RulesData> LoadAsync(CancellationToken cancellationToken = default);
}
