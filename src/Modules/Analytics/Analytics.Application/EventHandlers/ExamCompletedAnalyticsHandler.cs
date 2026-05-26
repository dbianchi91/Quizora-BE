using Analytics.Application.Interfaces;
using ExamEngine.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Analytics.Application.EventHandlers;

public sealed class ExamCompletedAnalyticsHandler(
    IAnalyticsRepository repo,
    ILeaderboardService leaderboard,
    ILogger<ExamCompletedAnalyticsHandler> logger)
    : INotificationHandler<ExamCompletedEvent>
{
    public async Task Handle(ExamCompletedEvent n, CancellationToken ct)
    {
        logger.LogInformation("Analytics handler started for session {SessionId}, user {UserId}, score {Score}",
            n.SessionId.Value, n.UserId.Value, n.NormalizedScore);

        var totalAnswered = n.CorrectCount + n.WrongCount;

        try
        {
            await repo.UpsertUserStatsAsync(
                n.UserId.Value, n.NormalizedScore,
                n.CorrectCount, totalAnswered, n.TotalTimeSpentSeconds, ct);
            logger.LogInformation("UpsertUserStats OK for user {UserId}", n.UserId.Value);
        }
        catch (Exception ex) { logger.LogError(ex, "UpsertUserStats FAILED for user {UserId}", n.UserId.Value); throw; }

        try
        {
            if (n.CategoryId.HasValue)
                await repo.UpsertCategoryStatsAsync(
                    n.UserId.Value, n.CategoryId.Value, n.NormalizedScore, ct);
            logger.LogInformation("UpsertCategoryStats OK for user {UserId}", n.UserId.Value);
        }
        catch (Exception ex) { logger.LogError(ex, "UpsertCategoryStats FAILED for user {UserId}", n.UserId.Value); throw; }

        try
        {
            await repo.UpsertDailyActivityAsync(
                n.UserId.Value, DateOnly.FromDateTime(DateTime.UtcNow),
                n.CorrectCount, n.TotalTimeSpentSeconds, ct);
            logger.LogInformation("UpsertDailyActivity OK for user {UserId}", n.UserId.Value);
        }
        catch (Exception ex) { logger.LogError(ex, "UpsertDailyActivity FAILED for user {UserId}", n.UserId.Value); throw; }

        try
        {
            await leaderboard.UpdateUserScoreAsync(
                n.UserId.Value, n.NormalizedScore, n.CategoryId, ct);
            logger.LogInformation("UpdateLeaderboard OK for user {UserId}", n.UserId.Value);
        }
        catch (Exception ex) { logger.LogError(ex, "UpdateLeaderboard FAILED for user {UserId}", n.UserId.Value); throw; }

        logger.LogInformation("Analytics handler completed for session {SessionId}", n.SessionId.Value);
    }
}
