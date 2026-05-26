using ExamEngine.Domain.ValueObjects;
using Identity.Domain.ValueObjects;
using Quizora.SharedKernel;

namespace ExamEngine.Domain.Events;

public sealed record ExamTimedOutEvent(
    Guid Id, DateTime OccurredOn,
    ExamSessionId SessionId, UserId UserId) : IDomainEvent;
