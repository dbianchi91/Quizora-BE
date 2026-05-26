using Identity.Domain.ValueObjects;
using MediatR;
using Quizora.SharedKernel;
using QuizManagement.Application.Interfaces;
using QuizManagement.Domain.ValueObjects;

namespace QuizManagement.Application.Commands.DeleteQuiz;

public sealed class DeleteQuizCommandHandler(IQuizManagementRepository repository)
    : IRequestHandler<DeleteQuizCommand, Result>
{
    public async Task<Result> Handle(DeleteQuizCommand request, CancellationToken ct)
    {
        var quiz = await repository.GetByIdAsync(QuizId.From(request.QuizId), ct);
        if (quiz is null) return Result.Failure(Error.NotFound("Quiz"));
        if (quiz.CreatorId != UserId.From(request.RequestingUserId))
            return Result.Failure(Error.Unauthorized());
        quiz.SoftDelete();
        await repository.SaveChangesAsync(ct);
        return Result.Success();
    }
}
