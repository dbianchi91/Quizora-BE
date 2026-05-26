using ExamEngine.Domain.ValueObjects;
using Identity.Domain.ValueObjects;
using Quizora.SharedKernel;

namespace ExamEngine.Domain.Events;

public sealed record ExamCompletedEvent(
    Guid Id,
    DateTime OccurredOn,
    ExamSessionId SessionId,
    UserId UserId,
    Guid QuizId,
    Guid? CategoryId,
    string? QuizTitle,
    double Score,
    double NormalizedScore,
    int CorrectCount,
    int WrongCount,
    int SkippedCount,
    int TotalTimeSpentSeconds,
    string SessionType) : IDomainEvent;
