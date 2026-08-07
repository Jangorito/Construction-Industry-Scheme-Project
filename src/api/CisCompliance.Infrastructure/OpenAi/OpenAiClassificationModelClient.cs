using System.ClientModel;
using CisCompliance.Core.Abstractions;
using CisCompliance.Core.Exceptions;
using OpenAI.Chat;

namespace CisCompliance.Infrastructure.OpenAi;

// Ported from call_model() in classify.py. This is the only file in the
// solution that references the OpenAI SDK's BinaryData/ChatMessage/
// ClientResultException types, everything else talks to
// IClassificationModelClient's plain-string interface instead.
public sealed class OpenAiClassificationModelClient : IClassificationModelClient
{
    private readonly ChatClient _chatClient;

    public OpenAiClassificationModelClient(ChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public async Task<string> GetClassificationCompletionAsync(
        string systemPrompt,
        string userMessage,
        string jsonSchema,
        string schemaName,
        CancellationToken cancellationToken = default)
    {
        List<ChatMessage> messages =
        [
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userMessage),
        ];

        // Temperature 0: this is a classification task where we want the
        // single most likely answer each time, not creative variety,
        // matching classify.py's call_model() exactly.
        var options = new ChatCompletionOptions
        {
            Temperature = 0f,
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                schemaName, BinaryData.FromString(jsonSchema), jsonSchemaIsStrict: true),
        };

        ChatCompletion completion;
        try
        {
            completion = await _chatClient.CompleteChatAsync(messages, options, cancellationToken);
        }
        catch (ClientResultException ex)
        {
            throw new ClassificationModelException("The OpenAI classification request failed.", ex);
        }

        if (completion.Content.Count == 0)
        {
            throw new ClassificationModelException(
                $"The model did not return usable content (finish reason: {completion.FinishReason}).");
        }

        return completion.Content[0].Text;
    }
}
