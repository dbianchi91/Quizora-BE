using AITutor.Domain.ValueObjects;
using Quizora.SharedKernel;

namespace AITutor.Domain.Entities;

public sealed class ChatMessage : BaseEntity<ChatMessageId>
{
    public ChatSessionId SessionId { get; private set; } = default!;
    public string Role { get; private set; } = default!;
    public string Content { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }

    private ChatMessage() { }

    public static ChatMessage Create(ChatSessionId sessionId, string role, string content) =>
        new()
        {
            Id = ChatMessageId.New(),
            SessionId = sessionId,
            Role = role,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };
}
