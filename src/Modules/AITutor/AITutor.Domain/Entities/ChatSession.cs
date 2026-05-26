using AITutor.Domain.ValueObjects;
using Identity.Domain.ValueObjects;
using Quizora.SharedKernel;

namespace AITutor.Domain.Entities;

public sealed class ChatSession : BaseEntity<ChatSessionId>
{
    public UserId UserId { get; private set; } = default!;
    public DateTime CreatedAt { get; private set; }
    public DateTime LastMessageAt { get; private set; }

    private readonly List<ChatMessage> _messages = [];
    public IReadOnlyList<ChatMessage> Messages => _messages.AsReadOnly();

    private ChatSession() { }

    public static ChatSession Create(UserId userId) => new()
    {
        Id = ChatSessionId.New(),
        UserId = userId,
        CreatedAt = DateTime.UtcNow,
        LastMessageAt = DateTime.UtcNow
    };

    public ChatMessage AddUserMessage(string content)
    {
        var msg = ChatMessage.Create(Id, "user", content);
        _messages.Add(msg);
        LastMessageAt = DateTime.UtcNow;
        return msg;
    }

    public ChatMessage AddAssistantMessage(string content)
    {
        var msg = ChatMessage.Create(Id, "assistant", content);
        _messages.Add(msg);
        LastMessageAt = DateTime.UtcNow;
        return msg;
    }
}
