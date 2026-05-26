namespace ExamEngine.Application.Interfaces;

public interface IExamTimerService
{
    Task ScheduleAsync(Guid sessionId, DateTime expiresAt, CancellationToken ct);
    Task CancelAsync(Guid sessionId, CancellationToken ct);
}
