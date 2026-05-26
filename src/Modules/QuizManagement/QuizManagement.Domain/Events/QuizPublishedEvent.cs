using Quizora.SharedKernel;
using QuizManagement.Domain.ValueObjects;

namespace QuizManagement.Domain.Events;

public sealed record QuizPublishedEvent(Guid Id, DateTime OccurredOn, QuizId QuizId) : IDomainEvent;
