using CisCompliance.Core.Abstractions;
using CisCompliance.Core.Models;

namespace CisCompliance.Tests.TestDoubles;

internal sealed class FakeRulesRepository : IRulesRepository
{
    private readonly RulesData _data;

    public FakeRulesRepository(RulesData data)
    {
        _data = data;
    }

    public Task<RulesData> LoadAsync(CancellationToken cancellationToken = default) => Task.FromResult(_data);
}
