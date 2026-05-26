using Quizora.SharedKernel;

namespace Identity.Domain.ValueObjects;

public sealed class Email : ValueObject
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Result<Email> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<Email>(Error.Validation("Email", "Email cannot be empty."));

        if (!value.Contains('@') || value.Length > 256)
            return Result.Failure<Email>(Error.Validation("Email", "Email is invalid."));

        return Result.Success(new Email(value.ToLowerInvariant()));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
