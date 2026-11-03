using System.Text.Json.Serialization;

namespace CisCompliance.Core.Models;

// Summary:
// Stores enums of the only two labels an invoice line item can have. 
// The JsonConverer attribute (de)serializes the enum to text instead 
//  of a number so it can be used by the API without C#'s enum to number 
//  convention.

// Serialization changes .NET objects to & from JSON

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ClassificationLabel
{
    Construction,
    Services,
}

