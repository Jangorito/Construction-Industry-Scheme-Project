using System.Text.Json.Serialization;

namespace CisCompliance.Core.Models;

// The model's own self report, not a calibrated accuracy score. "High"
// doesn't guarantee correct, it's a triage signal for which lines a human
// should sanity check first. See ClassificationPrompt for the exact rule.
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ConfidenceLevel
{
    High,
    Low,
}
