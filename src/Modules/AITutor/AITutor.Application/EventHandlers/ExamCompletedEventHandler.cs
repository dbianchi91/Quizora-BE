using AITutor.Application.Commands.GenerateStudyPlan;
using ExamEngine.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AITutor.Application.EventHandlers;

public sealed class ExamCompletedEventHandler(
    ISender sender,
    ILogger<ExamCompletedEventHandler> logger)
    : INotificationHandler<ExamCompletedEvent>
{
    public async Task Handle(ExamCompletedEvent notification, CancellationToken ct)
    {
        try
        {
            await sender.Send(new GenerateStudyPlanCommand(notification.UserId.Value, Automatic: true), ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Automatic study plan generation skipped for user {UserId}", notification.UserId.Value);
        }
    }
}
