using System.Text.Json.Serialization;

namespace CisCompliance.Core.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ClassificationLabel
{
    Construction,
    Services,
}
