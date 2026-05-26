using AITutor.Application.Interfaces;
using Anthropic.SDK;
using Anthropic.SDK.Messaging;
using Microsoft.Extensions.Configuration;

namespace AITutor.Infrastructure.Providers;

internal sealed class ClaudeAIProvider(IConfiguration configuration) : IAIProvider
{
    private readonly string _model = configuration["AI:Model"] ?? "claude-sonnet-4-6";
    private readonly int _maxTokens = int.Parse(configuration["AI:MaxTokens"] ?? "2048");
    private readonly string _apiKey = configuration["AI:ApiKey"]!;

    public async IAsyncEnumerable<string> StreamChatAsync(
        IReadOnlyList<Application.Interfaces.ChatMessageDto> history,
        string systemPrompt,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new InvalidOperationException("AI:ApiKey is not configured.");

        var client = new AnthropicClient(_apiKey);

        var messages = history.Select(m => new Message(
            m.Role == "user" ? RoleType.User : RoleType.Assistant,
            m.Content
        )).ToList();

        var request = new MessageParameters
        {
            Model = _model,
            MaxTokens = _maxTokens,
            Messages = messages,
            Stream = true,
            System = new List<SystemMessage> { new SystemMessage(systemPrompt) }
        };

        await foreach (var streamEvent in client.Messages.StreamClaudeMessageAsync(request, ct))
        {
            if (streamEvent.Delta?.Text is { Length: > 0 } text)
                yield return text;
        }
    }
}
