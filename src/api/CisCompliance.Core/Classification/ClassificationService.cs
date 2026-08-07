using System.Text.Json;
using CisCompliance.Core.Abstractions;
using CisCompliance.Core.Exceptions;
using CisCompliance.Core.Models;

namespace CisCompliance.Core.Classification;

// Orchestrates a classification request: load rules, build the prompt and
// schema, call the model, deserialize its answer, resolve citations. Mirrors
// classify.py's per-invoice loop in main(), minus the ground-truth scoring
// (there is no ground truth for a real production request).
public sealed class ClassificationService : IClassificationService
{
    private readonly IRulesRepository _rulesRepository;
    private readonly IClassificationModelClient _modelClient;

    public ClassificationService(IRulesRepository rulesRepository, IClassificationModelClient modelClient)
    {
        _rulesRepository = rulesRepository;
        _modelClient = modelClient;
    }

    public async Task<InvoiceClassificationResult> ClassifyAsync(
        InvoiceClassificationRequest request, CancellationToken cancellationToken = default)
    {
        var rules = await _rulesRepository.LoadAsync(cancellationToken);
        var systemPrompt = ClassificationPrompt.BuildSystemPrompt(rules.RulesText);
        var userMessage = ClassificationPrompt.BuildUserMessage(request);
        var schema = ClassificationSchemaBuilder.Build(rules.RuleIds);

        var raw = await _modelClient.GetClassificationCompletionAsync(
            systemPrompt, userMessage, schema, ClassificationSchemaBuilder.SchemaName, cancellationToken);

        ModelResponseDto? parsed;
        try
        {
            parsed = JsonSerializer.Deserialize<ModelResponseDto>(raw);
        }
        catch (JsonException ex)
        {
            throw new ClassificationModelException(
                "The model returned a response that did not match the expected schema.", ex);
        }

        if (parsed is null)
        {
            throw new ClassificationModelException("The model returned an empty response.");
        }

        var byLineId = parsed.LineItems.ToDictionary(li => li.LineId);
        var results = new List<LineItemClassification>(request.LineItems.Count);

        // Fail loud on a missing line item rather than silently skipping it.
        // classify.py records this as a scoring "miss" because it has ground
        // truth to fall back on, a production request has none, so a
        // dropped line item here means the caller gets an incomplete
        // response with no way to know that unless we say so.
        foreach (var requested in request.LineItems)
        {
            if (!byLineId.TryGetValue(requested.LineId, out var predicted))
            {
                throw new ClassificationModelException(
                    $"The model did not return a classification for line item '{requested.LineId}'.");
            }

            var citations = CitationResolver.Resolve(predicted.RuleIds, rules.RulesById);
            results.Add(new LineItemClassification(
                requested.LineId,
                ParseLabel(predicted.Label),
                ParseConfidence(predicted.Confidence),
                predicted.Reasoning,
                citations));
        }

        return new InvoiceClassificationResult(request.InvoiceId, results);
    }

    private static ConfidenceLevel ParseConfidence(string value) => value switch
    {
        "high" => ConfidenceLevel.High,
        "low" => ConfidenceLevel.Low,
        _ => throw new ClassificationModelException($"Unexpected confidence value '{value}' returned by the model."),
    };

    private static ClassificationLabel ParseLabel(string value) => value switch
    {
        "Construction" => ClassificationLabel.Construction,
        "Services" => ClassificationLabel.Services,
        _ => throw new ClassificationModelException($"Unexpected label value '{value}' returned by the model."),
    };
}
