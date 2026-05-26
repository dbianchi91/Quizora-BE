using Identity.Domain.ValueObjects;
using Quizora.SharedKernel;

namespace Identity.Domain.Events;

public sealed record UserRegisteredEvent(
    Guid Id,
    DateTime OccurredOn,
    UserId UserId,
    string Email) : IDomainEvent;
