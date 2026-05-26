using FluentValidation;

namespace QuizManagement.Application.Commands.CreateQuestion;

public sealed class CreateQuestionCommandValidator : AbstractValidator<CreateQuestionCommand>
{
    public CreateQuestionCommandValidator()
    {
        RuleFor(x => x.Text).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Options).Must(o => o.Count >= 2).WithMessage("At least 2 options required.");
        RuleFor(x => x.Options).Must(o => o.Any(opt => opt.IsCorrect)).WithMessage("At least one correct option.");
    }
}
