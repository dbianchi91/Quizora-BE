namespace QuizManagement.Domain.ValueObjects;

public record QuestionId(Guid Value)
{
    public static QuestionId New() => new(Guid.NewGuid());
    public static QuestionId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
