using CisCompliance.Core.Models;

namespace CisCompliance.Core.Abstractions;

// Summary:
// Promises to load the CIS rules. Called by ClassificationService who 
//  doesn't care how the rules are loaded. The real app uses 
//  FileRulesRepository, but tests can use the in-memory implementation.
public interface IRulesRepository
{
    Task<RulesData> LoadAsync(CancellationToken cancellationToken = default);
}
