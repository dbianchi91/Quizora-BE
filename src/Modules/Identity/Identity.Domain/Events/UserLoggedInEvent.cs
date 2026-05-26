using Identity.Domain.ValueObjects;
using Quizora.SharedKernel;

namespace Identity.Domain.Events;

public sealed record UserLoggedInEvent(
    Guid Id,
    DateTime OccurredOn,
    UserId UserId) : IDomainEvent;
