namespace ExamEngine.Domain.ValueObjects;

public record ExamSessionId(Guid Value)
{
    public static ExamSessionId New() => new(Guid.NewGuid());
    public static ExamSessionId From(Guid v) => new(v);
    public override string ToString() => Value.ToString();
}
