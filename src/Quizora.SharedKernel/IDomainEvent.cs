using MediatR;

namespace Quizora.SharedKernel;

public interface IDomainEvent : INotification
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
}
