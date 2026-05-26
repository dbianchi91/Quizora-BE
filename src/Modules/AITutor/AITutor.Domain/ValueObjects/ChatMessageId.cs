namespace AITutor.Domain.ValueObjects;

public record ChatMessageId(Guid Value)
{
    public static ChatMessageId New() => new(Guid.NewGuid());
    public static ChatMessageId From(Guid v) => new(v);
    public override string ToString() => Value.ToString();
}
