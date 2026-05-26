namespace Analytics.Application.Interfaces;

public interface ILeaderboardService
{
    Task UpdateUserScoreAsync(Guid userId, double normalizedScore,
        Guid? categoryId, CancellationToken ct);
}
