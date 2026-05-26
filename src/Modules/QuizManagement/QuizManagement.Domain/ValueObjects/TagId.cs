namespace QuizManagement.Domain.ValueObjects;

public record TagId(Guid Value)
{
    public static TagId New() => new(Guid.NewGuid());
    public static TagId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
