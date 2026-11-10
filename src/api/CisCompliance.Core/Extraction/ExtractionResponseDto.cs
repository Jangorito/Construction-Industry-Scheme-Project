using System.Text.Json.Serialization;

namespace CisCompliance.Core.Extraction;

// Wire shape of the model's raw extraction JSON, internal-only, used inside
// InvoiceExtractionService before mapping to the public
// InvoiceClassificationRequest. Nullable fields mirror the schema's
// nullable unions, see InvoiceExtractionSchemaBuilder.
internal sealed record ExtractionResponseDto(
    [property: JsonPropertyName("invoice_number")] string? InvoiceNumber,
    [property: JsonPropertyName("supplier")] string Supplier,
    [property: JsonPropertyName("customer")] string? Customer,
    [property: JsonPropertyName("date")] string? Date,
    [property: JsonPropertyName("line_items")] IReadOnlyList<ExtractionLineItemDto> LineItems);

internal sealed record ExtractionLineItemDto(
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("amount_gbp")] decimal AmountGbp);
