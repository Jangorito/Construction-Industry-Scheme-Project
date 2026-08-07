using System.Text.Json;
using CisCompliance.Core.Abstractions;
using CisCompliance.Core.Models;
using Microsoft.Extensions.Options;

namespace CisCompliance.Infrastructure.Rules;

// Ported from load_rules() in classify.py. Reads cis_rules.md (folded
// directly into the system prompt) and cis_rules_index.json (used to build
// the schema's rule_ids enum and to resolve citations afterward).
//
// Loaded once and memoized via Lazy<Task<T>>, mirroring load_rules() being
// called once per process in classify.py's main(). Lazy<Task<T>> rather than
// a plain cached field so concurrent first-requests don't race to read the
// files twice, and rather than eager loading in the constructor so a
// missing/malformed file surfaces as a failed request, not a crash at
// startup before anyone has asked for anything.
public sealed class FileRulesRepository : IRulesRepository
{
    private readonly RulesRepositoryOptions _options;
    private readonly Lazy<Task<RulesData>> _rulesData;

    public FileRulesRepository(IOptions<RulesRepositoryOptions> options)
    {
        _options = options.Value;
        _rulesData = new Lazy<Task<RulesData>>(LoadFromDiskAsync, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public Task<RulesData> LoadAsync(CancellationToken cancellationToken = default) => _rulesData.Value;

    private async Task<RulesData> LoadFromDiskAsync()
    {
        var rulesText = await File.ReadAllTextAsync(_options.RulesMarkdownPath);
        var indexJson = await File.ReadAllTextAsync(_options.RulesIndexPath);

        var index = JsonSerializer.Deserialize<RulesIndexFileDto>(indexJson)
            ?? throw new InvalidOperationException($"Failed to parse {_options.RulesIndexPath}.");

        var ruleIds = index.Rules.Select(r => r.Id).ToList();
        var rulesById = index.Rules.ToDictionary(
            r => r.Id,
            r => new RuleDefinition(r.Id, r.Title, r.Category, r.SourceType, r.SourceName, r.SourceTitle, r.SourceUrl, r.Verified));

        return new RulesData(rulesText, ruleIds, rulesById);
    }
}
