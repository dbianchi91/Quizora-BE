namespace QuizManagement.Domain.ValueObjects;

public record QuizId(Guid Value)
{
    public static QuizId New() => new(Guid.NewGuid());
    public static QuizId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
