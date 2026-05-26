using ExamEngine.Application.Commands.AutoSubmitExam;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ExamEngine.Infrastructure.Workers;

public sealed class ExamTimerWorker(
    IConnectionMultiplexer redis,
    IServiceScopeFactory scopeFactory,
    ILogger<ExamTimerWorker> logger) : BackgroundService
{
    private const string TimerKey = "exam:timers";
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("ExamTimerWorker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcessExpiredSessionsAsync(stoppingToken);
            await Task.Delay(PollingInterval, stoppingToken);
        }
    }

    private async Task ProcessExpiredSessionsAsync(CancellationToken ct)
    {
        try
        {
            var db = redis.GetDatabase();
            var nowScore = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var expired = await db.SortedSetRangeByScoreAsync(
                TimerKey, stop: nowScore, take: 100);

            foreach (var entry in expired)
            {
                if (!Guid.TryParse(entry, out var sessionId)) continue;

                using var scope = scopeFactory.CreateScope();
                var sender = scope.ServiceProvider.GetRequiredService<ISender>();
                var result = await sender.Send(new AutoSubmitExamCommand(sessionId), ct);

                if (result.IsSuccess)
                {
                    await db.SortedSetRemoveAsync(TimerKey, entry);
                    logger.LogInformation("Auto-submitted expired session {SessionId}", sessionId);
                }
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Error processing expired exam sessions");
        }
    }
}
