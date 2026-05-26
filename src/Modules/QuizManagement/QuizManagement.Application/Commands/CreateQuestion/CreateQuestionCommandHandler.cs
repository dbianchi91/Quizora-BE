using Identity.Domain.ValueObjects;
using MediatR;
using Quizora.SharedKernel;
using QuizManagement.Application.DTOs;
using QuizManagement.Application.Interfaces;
using QuizManagement.Domain.Entities;
using QuizManagement.Domain.Enums;

namespace QuizManagement.Application.Commands.CreateQuestion;

public sealed class CreateQuestionCommandHandler(IQuizManagementRepository repository)
    : IRequestHandler<CreateQuestionCommand, Result<QuestionDto>>
{
    public async Task<Result<QuestionDto>> Handle(CreateQuestionCommand request, CancellationToken ct)
    {
        if (!Enum.TryParse<DifficultyLevel>(request.Difficulty, out var difficulty))
            return Result.Failure<QuestionDto>(Error.Validation("Difficulty", "Invalid difficulty level."));

        var questionResult = Question.Create(
            request.Text, difficulty, UserId.From(request.CreatorId),
            request.Options.Select(o => (o.Text, o.IsCorrect)), request.Explanation);

        if (questionResult.IsFailure) return Result.Failure<QuestionDto>(questionResult.Error);

        await repository.AddQuestionAsync(questionResult.Value, ct);
        await repository.SaveChangesAsync(ct);

        var q = questionResult.Value;
        return Result.Success(new QuestionDto(q.Id.Value, q.Text, q.Explanation, q.Difficulty.ToString(),
            q.Options.Select(o => new OptionDto(o.Id.Value, o.Text, o.OrderIndex)).ToList().AsReadOnly(),
            DateTime.UtcNow));
    }
}
