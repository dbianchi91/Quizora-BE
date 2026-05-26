using Analytics.Application.Interfaces;
using StackExchange.Redis;

namespace Analytics.Infrastructure.Services;

public sealed class LeaderboardService(IConnectionMultiplexer redis) : ILeaderboardService
{
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task UpdateUserScoreAsync(Guid userId, double normalizedScore,
        Guid? categoryId, CancellationToken ct)
    {
        var member = userId.ToString();
        var currentGlobal = await _db.SortedSetScoreAsync("leaderboard:global", member);
        if (currentGlobal is null || normalizedScore > currentGlobal.Value)
        {
            await _db.SortedSetAddAsync("leaderboard:global", member, normalizedScore);
            if (categoryId.HasValue)
                await _db.SortedSetAddAsync($"leaderboard:category:{categoryId.Value}", member, normalizedScore);
        }
    }
}
