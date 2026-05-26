using FluentValidation;

namespace QuizManagement.Application.Commands.CreateQuiz;

public sealed class CreateQuizCommandValidator : AbstractValidator<CreateQuizCommand>
{
    public CreateQuizCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(200)
            .Matches("^[a-z0-9-]+$").WithMessage("Slug can only contain lowercase letters, numbers, and hyphens.");
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.TimeLimitSeconds).GreaterThan(0);
        RuleFor(x => x.PointsCorrect).GreaterThan(0);
        RuleFor(x => x.PointsWrong).LessThanOrEqualTo(0);
    }
}
