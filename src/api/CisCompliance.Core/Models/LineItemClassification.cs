namespace CisCompliance.Core.Models;

// Summary:
// Represents AI model's output of a classified line item in an invoice 
//  including its label and citation(s) (that was handled by the ClassificationService).
// Created by the ClassificationService
// There's no desc or amount because Angular sent the request with those 
//  properties already, this shape is just the model's answer to the 
//  classification request.
public sealed record LineItemClassification(
string LineId,
    ClassificationLabel Label,
    ConfidenceLevel Confidence,
    string Reasoning,
    IReadOnlyList<RuleCitation> Citations);
