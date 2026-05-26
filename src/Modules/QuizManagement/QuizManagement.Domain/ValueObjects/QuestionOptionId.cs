namespace QuizManagement.Domain.ValueObjects;

public record QuestionOptionId(Guid Value)
{
    public static QuestionOptionId New() => new(Guid.NewGuid());
    public static QuestionOptionId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
