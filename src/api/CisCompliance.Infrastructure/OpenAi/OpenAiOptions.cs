namespace CisCompliance.Infrastructure.OpenAi;

public sealed class OpenAiOptions
{
    // Deliberately no default. A blank key must fail fast at startup, not
    // fall back to some placeholder that produces a confusing 401 later.
    public string ApiKey { get; set; } = string.Empty;

    // Matches classify.py's DEFAULT_MODEL, the model the 95.7% validated
    // accuracy figure was measured against.
    public string Model { get; set; } = "gpt-4o-mini";
}
