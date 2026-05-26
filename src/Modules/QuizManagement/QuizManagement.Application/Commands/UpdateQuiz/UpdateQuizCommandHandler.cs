using Identity.Domain.ValueObjects;
using MediatR;
using Quizora.SharedKernel;
using QuizManagement.Application.Interfaces;
using QuizManagement.Domain.Entities;
using QuizManagement.Domain.ValueObjects;

namespace QuizManagement.Application.Commands.UpdateQuiz;

public sealed class UpdateQuizCommandHandler(IQuizManagementRepository repository)
    : IRequestHandler<UpdateQuizCommand, Result>
{
    public async Task<Result> Handle(UpdateQuizCommand request, CancellationToken ct)
    {
        var quiz = await repository.GetByIdAsync(QuizId.From(request.QuizId), ct);
        if (quiz is null) return Result.Failure(Error.NotFound("Quiz"));
        if (quiz.CreatorId != UserId.From(request.RequestingUserId))
            return Result.Failure(Error.Unauthorized());

        var config = ExamConfig.Create(request.TimeLimitSeconds, request.PointsCorrect,
            request.PointsWrong, request.PointsSkipped, request.PassingScore,
            request.ShuffleQuestions, request.ShuffleOptions);
        quiz.UpdateDetails(request.Title, request.Slug, request.Description,
            CategoryId.From(request.CategoryId), config);
        await repository.SaveChangesAsync(ct);
        return Result.Success();
    }
}
