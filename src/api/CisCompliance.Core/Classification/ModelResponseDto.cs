using System.Text.Json.Serialization;

namespace CisCompliance.Core.Classification;

// Wire shape of the model's raw JSON response, used only inside
// ClassificationService to deserialize before mapping to the public
// LineItemClassification model. Confidence/Label stay plain strings here
// rather than the public enums, deserialization is mapped explicitly in
// ClassificationService so an unexpected value fails loudly with a clear
// message instead of a JsonException pointing at the wrong layer.
internal sealed record ModelResponseDto(
    [property: JsonPropertyName("line_items")] IReadOnlyList<ModelLineItemDto> LineItems);

internal sealed record ModelLineItemDto(
    [property: JsonPropertyName("line_id")] string LineId,
    [property: JsonPropertyName("reasoning")] string Reasoning,
    [property: JsonPropertyName("rule_ids")] IReadOnlyList<string> RuleIds,
    [property: JsonPropertyName("confidence")] string Confidence,
    [property: JsonPropertyName("label")] string Label);
