using FluentValidation;

namespace ExamEngine.Application.Commands.StartExam;

public sealed class StartExamCommandValidator : AbstractValidator<StartExamCommand>
{
    public StartExamCommandValidator()
    {
        RuleFor(x => x.QuizId).NotEmpty();
        RuleFor(x => x.SessionType).NotEmpty()
            .Must(t => new[] { "Official", "Simulation", "Study" }.Contains(t))
            .WithMessage("SessionType must be Official, Simulation, or Study.");
        RuleFor(x => x.IdempotencyKey).NotEmpty();
    }
}
