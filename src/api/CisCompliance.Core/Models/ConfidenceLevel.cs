using System.Text.Json.Serialization;

namespace CisCompliance.Core.Models;

// Summary:
// Enums of self-reported level of confidence the model has in a classification. 
// It doesn't mean correct, but will show low if:
//  - line item is ambiguous
//  - if the mixed-contract override was used
//  - two rules presented conflicting classifications
// In these events the classification isn't wrong, the signal just prompts a human
//  traige check. 
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ConfidenceLevel
{
    High,
    Low,
}
