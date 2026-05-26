using ExamEngine.Application.Interfaces;
using ExamEngine.Domain.Entities;
using ExamEngine.Domain.Enums;
using Identity.Domain.ValueObjects;
using MediatR;
using Quizora.SharedKernel;

namespace ExamEngine.Application.Commands.StartExam;

public sealed class StartExamCommandHandler(
    IExamRepository examRepo,
    IQuizReader quizReader,
    IExamTimerService timerService) : IRequestHandler<StartExamCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(StartExamCommand request, CancellationToken ct)
    {
        if (!Enum.TryParse<SessionType>(request.SessionType, out var sessionType))
            return Result.Failure<Guid>(Error.Validation("SessionType", "Invalid session type."));

        if (sessionType == SessionType.Official)
        {
            var existing = await examRepo.GetActiveOfficialSessionAsync(
                request.QuizId, UserId.From(request.UserId), ct);
            if (existing is not null)
                return Result.Success(existing.Id.Value);
        }

        var quiz = await quizReader.GetQuizForExamAsync(request.QuizId, ct);
        if (quiz is null) return Result.Failure<Guid>(Error.NotFound("Quiz"));

        var config = ConfigSnapshot.From(
            quiz.PointsCorrect, quiz.PointsWrong, quiz.PointsSkipped,
            quiz.TimeLimitSeconds, quiz.PassingScore,
            quiz.ShuffleQuestions, quiz.ShuffleOptions,
            quiz.CategoryId, quiz.Title);

        var session = ExamSession.Start(
            request.QuizId, UserId.From(request.UserId),
            sessionType, config, quiz.TotalQuestions);

        await examRepo.AddAsync(session, ct);
        await examRepo.SaveChangesAsync(ct);

        if (sessionType != SessionType.Study && session.ExpiresAt.HasValue)
            await timerService.ScheduleAsync(session.Id.Value, session.ExpiresAt.Value, ct);

        return Result.Success(session.Id.Value);
    }
}
