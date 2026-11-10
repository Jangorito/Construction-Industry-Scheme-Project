using System.Text.Json.Nodes;

namespace CisCompliance.Core.Extraction;

// Unlike ClassificationSchemaBuilder, property order here is NOT
// load-bearing. The classification schema's reasoning-before-label order
// matters because the model works through a genuine multi-step reasoning
// procedure and must not commit to an answer before reasoning it out.
// Extraction has no equivalent reasoning step, it's direct transcription of
// text/numbers already visible on the document, so there's no "commit
// early, justify late" failure mode to guard against here. Any property
// order would behave identically. Still built with JsonObject rather than
// a serialized POCO for consistency with the rest of this layer, not
// because order matters this time.
public static class InvoiceExtractionSchemaBuilder
{
    public const string SchemaName = "cis_invoice_extraction";

    public static string Build()
    {
        var schema = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                // Nullable: the model must say "not on the document" rather
                // than invent a value, see InvoiceExtractionPrompt.
                ["invoice_number"] = new JsonObject { ["type"] = new JsonArray("string", "null") },
                ["supplier"] = new JsonObject { ["type"] = "string" },
                ["customer"] = new JsonObject { ["type"] = new JsonArray("string", "null") },
                ["date"] = new JsonObject { ["type"] = new JsonArray("string", "null") },
                ["line_items"] = new JsonObject
                {
                    ["type"] = "array",
                    ["items"] = new JsonObject
                    {
                        ["type"] = "object",
                        ["properties"] = new JsonObject
                        {
                            ["description"] = new JsonObject { ["type"] = "string" },
                            ["amount_gbp"] = new JsonObject { ["type"] = "number" },
                        },
                        ["required"] = new JsonArray("description", "amount_gbp"),
                        ["additionalProperties"] = false,
                    },
                },
            },
            ["required"] = new JsonArray("invoice_number", "supplier", "customer", "date", "line_items"),
            ["additionalProperties"] = false,
        };

        return schema.ToJsonString();
    }
}
