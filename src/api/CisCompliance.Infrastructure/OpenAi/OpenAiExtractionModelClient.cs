using System.ClientModel;
using CisCompliance.Core.Abstractions;
using CisCompliance.Core.Exceptions;
using OpenAI.Chat;

namespace CisCompliance.Infrastructure.OpenAi;

// Reuses the same ChatClient singleton registered for classification.
// gpt-4o-mini already supports both PDF file input and image input in
// Chat Completions, so no second model or second client registration is
// needed, just a different call shape. This is the only file that
// references the OpenAI SDK's multimodal content-part types, same
// discipline as OpenAiClassificationModelClient.
public sealed class OpenAiExtractionModelClient : IExtractionModelClient
{
    private readonly ChatClient _chatClient;

    public OpenAiExtractionModelClient(ChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public async Task<string> GetExtractionCompletionAsync(
        string systemPrompt,
        string userInstruction,
        byte[] fileBytes,
        string mediaType,
        string fileName,
        string jsonSchema,
        string schemaName,
        CancellationToken cancellationToken = default)
    {
        // CreateFilePart is marked [Experimental("OPENAI001")] by the SDK,
        // it's the OpenAI API's file-input feature that's evaluation-only,
        // not a bug in how it's used here, PDF input to Chat Completions
        // is a real, documented, shipped OpenAI capability.
#pragma warning disable OPENAI001
        ChatMessageContentPart filePart = mediaType switch
        {
            "application/pdf" => ChatMessageContentPart.CreateFilePart(BinaryData.FromBytes(fileBytes), mediaType, fileName),
            // High detail deliberately: invoices carry small numeric text
            // (amounts, dates) where a misread is exactly the failure mode
            // this feature exists to avoid.
            "image/png" or "image/jpeg" => ChatMessageContentPart.CreateImagePart(
                BinaryData.FromBytes(fileBytes), mediaType, ChatImageDetailLevel.High),
            _ => throw new InvoiceExtractionException($"Unsupported media type '{mediaType}' for extraction."),
        };
#pragma warning restore OPENAI001

        List<ChatMessage> messages =
        [
            new SystemChatMessage(systemPrompt),
            new UserChatMessage([ChatMessageContentPart.CreateTextPart(userInstruction), filePart]),
        ];

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
            throw new InvoiceExtractionException("The OpenAI extraction request failed.", ex);
        }

        if (completion.Content.Count == 0)
        {
            throw new InvoiceExtractionException(
                $"The model did not return usable content (finish reason: {completion.FinishReason}).");
        }

        return completion.Content[0].Text;
    }
}
