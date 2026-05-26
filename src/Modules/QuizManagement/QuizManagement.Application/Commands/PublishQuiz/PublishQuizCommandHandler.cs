using Identity.Domain.ValueObjects;
using MediatR;
using Quizora.SharedKernel;
using QuizManagement.Application.Interfaces;
using QuizManagement.Domain.ValueObjects;

namespace QuizManagement.Application.Commands.PublishQuiz;

public sealed class PublishQuizCommandHandler(IQuizManagementRepository repository)
    : IRequestHandler<PublishQuizCommand, Result>
{
    public async Task<Result> Handle(PublishQuizCommand request, CancellationToken ct)
    {
        var quiz = await repository.GetByIdAsync(QuizId.From(request.QuizId), ct);
        if (quiz is null) return Result.Failure(Error.NotFound("Quiz"));

        if (quiz.CreatorId != UserId.From(request.RequestingUserId))
            return Result.Failure(Error.Unauthorized());

        var result = quiz.Publish();
        if (result.IsFailure) return result;

        await repository.SaveChangesAsync(ct);
        return Result.Success();
    }
}
