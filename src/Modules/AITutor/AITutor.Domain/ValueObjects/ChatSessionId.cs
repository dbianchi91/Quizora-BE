namespace AITutor.Domain.ValueObjects;

public record ChatSessionId(Guid Value)
{
    public static ChatSessionId New() => new(Guid.NewGuid());
    public static ChatSessionId From(Guid v) => new(v);
    public override string ToString() => Value.ToString();
}
