using Identity.Domain.ValueObjects;
using MediatR;
using Quizora.SharedKernel;
using QuizManagement.Application.Interfaces;
using QuizManagement.Domain.ValueObjects;

namespace QuizManagement.Application.Commands.AddQuestionToQuiz;

public sealed class AddQuestionToQuizCommandHandler(IQuizManagementRepository repository)
    : IRequestHandler<AddQuestionToQuizCommand, Result>
{
    public async Task<Result> Handle(AddQuestionToQuizCommand request, CancellationToken ct)
    {
        var quiz = await repository.GetByIdAsync(QuizId.From(request.QuizId), ct);
        if (quiz is null) return Result.Failure(Error.NotFound("Quiz"));
        if (quiz.CreatorId != UserId.From(request.RequestingUserId))
            return Result.Failure(Error.Unauthorized());

        var question = await repository.GetQuestionByIdAsync(QuestionId.From(request.QuestionId), ct);
        if (question is null) return Result.Failure(Error.NotFound("Question"));

        quiz.AddQuestion(QuestionId.From(request.QuestionId));
        await repository.SaveChangesAsync(ct);
        return Result.Success();
    }
}
