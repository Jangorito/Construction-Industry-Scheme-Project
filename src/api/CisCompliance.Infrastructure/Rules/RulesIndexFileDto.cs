using System.Text.Json.Serialization;

namespace CisCompliance.Infrastructure.Rules;

// Matches cis_rules_index.json verbatim (snake_case keys, as written by the
// POC). Internal: FileRulesRepository maps this into the Core-owned
// RuleDefinition/RulesData types, nothing outside this project sees the
// file's raw shape.
internal sealed record RulesIndexFileDto(
    [property: JsonPropertyName("rules")] IReadOnlyList<RulesIndexRuleDto> Rules);

internal sealed record RulesIndexRuleDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("category")] string Category,
    [property: JsonPropertyName("source_type")] string SourceType,
    [property: JsonPropertyName("source_name")] string SourceName,
    [property: JsonPropertyName("source_title")] string SourceTitle,
    [property: JsonPropertyName("source_url")] string? SourceUrl,
    [property: JsonPropertyName("verified")] string Verified);
