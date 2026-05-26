using ExamEngine.Application.Interfaces;
using StackExchange.Redis;

namespace ExamEngine.Infrastructure.Services;

internal sealed class ExamTimerService(IConnectionMultiplexer redis) : IExamTimerService
{
    private const string TimerKey = "exam:timers";

    public async Task ScheduleAsync(Guid sessionId, DateTime expiresAt, CancellationToken ct)
    {
        var db = redis.GetDatabase();
        var score = new DateTimeOffset(expiresAt).ToUnixTimeMilliseconds();
        await db.SortedSetAddAsync(TimerKey, sessionId.ToString(), score);
    }

    public async Task CancelAsync(Guid sessionId, CancellationToken ct)
    {
        var db = redis.GetDatabase();
        await db.SortedSetRemoveAsync(TimerKey, sessionId.ToString());
    }
}
