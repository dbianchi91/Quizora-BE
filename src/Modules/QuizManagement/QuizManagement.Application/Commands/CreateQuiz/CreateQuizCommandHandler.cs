using Identity.Domain.ValueObjects;
using MediatR;
using Quizora.SharedKernel;
using QuizManagement.Application.DTOs;
using QuizManagement.Application.Interfaces;
using QuizManagement.Domain.Entities;
using QuizManagement.Domain.ValueObjects;

namespace QuizManagement.Application.Commands.CreateQuiz;

public sealed class CreateQuizCommandHandler(IQuizManagementRepository repository)
    : IRequestHandler<CreateQuizCommand, Result<QuizSummaryDto>>
{
    public async Task<Result<QuizSummaryDto>> Handle(CreateQuizCommand request, CancellationToken ct)
    {
        if (await repository.SlugExistsAsync(request.Slug, ct))
            return Result.Failure<QuizSummaryDto>(Error.Conflict("Slug"));

        var quizResult = Quiz.Create(
            request.Title, request.Slug,
            CategoryId.From(request.CategoryId),
            UserId.From(request.CreatorId),
            request.Description);

        if (quizResult.IsFailure) return Result.Failure<QuizSummaryDto>(quizResult.Error);

        var quiz = quizResult.Value;
        var config = ExamConfig.Create(request.TimeLimitSeconds, request.PointsCorrect,
            request.PointsWrong, request.PointsSkipped, request.PassingScore,
            request.ShuffleQuestions, request.ShuffleOptions);
        quiz.UpdateDetails(quiz.Title, quiz.Slug, quiz.Description, quiz.CategoryId, config);

        await repository.AddQuizAsync(quiz, ct);
        await repository.SaveChangesAsync(ct);

        return Result.Success(new QuizSummaryDto(quiz.Id.Value, quiz.Title, quiz.Slug,
            quiz.Description, quiz.Status.ToString(), quiz.CategoryId.Value, "", 0,
            new ExamConfigDto(request.TimeLimitSeconds, request.PointsCorrect, request.PointsWrong,
                request.PointsSkipped, request.PassingScore, request.ShuffleQuestions, request.ShuffleOptions),
            quiz.CreatedAt));
    }
}
