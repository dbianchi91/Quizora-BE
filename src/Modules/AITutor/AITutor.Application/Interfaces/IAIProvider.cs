namespace AITutor.Application.Interfaces;

public record ChatMessageDto(string Role, string Content);

public interface IAIProvider
{
    IAsyncEnumerable<string> StreamChatAsync(
        IReadOnlyList<ChatMessageDto> history,
        string systemPrompt,
        CancellationToken ct);
}
