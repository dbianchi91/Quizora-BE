using System.ClientModel;
using System.Runtime.CompilerServices;
using AITutor.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;

namespace AITutor.Infrastructure.Providers;

internal sealed class OpenAiCompatibleAIProvider(IConfiguration configuration) : IAIProvider
{
    private readonly string _baseUrl = configuration["AI:BaseUrl"]
        ?? throw new InvalidOperationException("AI:BaseUrl is not configured.");
    private readonly string _model = configuration["AI:Model"]
        ?? throw new InvalidOperationException("AI:Model is not configured.");
    private readonly string _apiKey = string.IsNullOrWhiteSpace(configuration["AI:ApiKey"])
        ? "not-needed"
        : configuration["AI:ApiKey"]!;
    private readonly int _maxTokens = int.Parse(configuration["AI:MaxTokens"] ?? "2048");

    public async IAsyncEnumerable<string> StreamChatAsync(
        IReadOnlyList<ChatMessageDto> history,
        string systemPrompt,
        [EnumeratorCancellation] CancellationToken ct)
    {
        var client = new ChatClient(
            _model,
            new ApiKeyCredential(_apiKey),
            new OpenAIClientOptions { Endpoint = new Uri(_baseUrl) });

        var messages = new List<ChatMessage> { new SystemChatMessage(systemPrompt) };
        foreach (var m in history)
        {
            messages.Add(m.Role == "user"
                ? new UserChatMessage(m.Content)
                : new AssistantChatMessage(m.Content));
        }

        var options = new ChatCompletionOptions { MaxOutputTokenCount = _maxTokens };

        await foreach (var update in client.CompleteChatStreamingAsync(messages, options, ct))
        {
            foreach (var part in update.ContentUpdate)
            {
                if (!string.IsNullOrEmpty(part.Text))
                    yield return part.Text;
            }
        }
    }
}
