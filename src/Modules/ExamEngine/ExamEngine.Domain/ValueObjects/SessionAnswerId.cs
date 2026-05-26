namespace ExamEngine.Domain.ValueObjects;

public record SessionAnswerId(Guid Value)
{
    public static SessionAnswerId New() => new(Guid.NewGuid());
    public static SessionAnswerId From(Guid v) => new(v);
    public override string ToString() => Value.ToString();
}
