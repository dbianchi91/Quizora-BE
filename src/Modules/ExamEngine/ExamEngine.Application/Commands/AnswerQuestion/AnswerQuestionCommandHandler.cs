using ExamEngine.Application.DTOs;
using ExamEngine.Application.Interfaces;
using ExamEngine.Domain.Enums;
using ExamEngine.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using Quizora.SharedKernel;

namespace ExamEngine.Application.Commands.AnswerQuestion;

public sealed class AnswerQuestionCommandHandler(
    IExamRepository examRepo, IQuizReader quizReader, IPublisher publisher,
    ILogger<AnswerQuestionCommandHandler> logger)
    : IRequestHandler<AnswerQuestionCommand, Result<AnswerFeedbackDto>>
{
    public async Task<Result<AnswerFeedbackDto>> Handle(AnswerQuestionCommand request, CancellationToken ct)
    {
        var session = await examRepo.GetByIdAsync(ExamSessionId.From(request.SessionId), ct);
        if (session is null) return Result.Failure<AnswerFeedbackDto>(Error.NotFound("ExamSession"));

        if (session.ExpiresAt.HasValue && DateTime.UtcNow > session.ExpiresAt.Value)
        {
            session.AutoSubmit();
            var timedOutEvents = session.DomainEvents.ToList();
            session.ClearDomainEvents();
            await examRepo.SaveChangesAsync(ct);
            foreach (var ev in timedOutEvents)
            {
                try { await publisher.Publish(ev, ct); }
                catch (Exception ex) { logger.LogError(ex, "Failed to publish domain event {EventType}", ev.GetType().Name); }
            }
            return Result.Failure<AnswerFeedbackDto>(Error.Validation("Timer", "Session has expired."));
        }

        bool isCorrect = false;
        Guid? correctOptionId = null;
        string? explanation = null;

        if (request.SelectedOptionId.HasValue)
        {
            var quiz = await quizReader.GetQuizForExamAsync(session.QuizId, ct);
            var question = quiz?.Questions.FirstOrDefault(q => q.QuestionId == request.QuestionId);
            var correctOption = question?.Options.FirstOrDefault(o => o.IsCorrect);
            correctOptionId = correctOption?.OptionId;
            isCorrect = correctOption?.OptionId == request.SelectedOptionId;
            explanation = question?.Explanation;
        }

        var result = session.RecordAnswer(request.QuestionId, request.SelectedOptionId,
            isCorrect, request.TimeSpentSeconds);
        if (result.IsFailure) return Result.Failure<AnswerFeedbackDto>(result.Error);

        await examRepo.SaveChangesAsync(ct);

        var feedback = new AnswerFeedbackDto(
            isCorrect, result.Value.PointsAwarded,
            session.Type == SessionType.Study ? explanation : null,
            session.Type == SessionType.Study ? correctOptionId : null);

        return Result.Success(feedback);
    }
}
