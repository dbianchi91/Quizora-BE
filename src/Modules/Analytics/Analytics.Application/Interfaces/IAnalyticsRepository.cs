namespace Analytics.Application.Interfaces;

public interface IAnalyticsRepository
{
    Task UpsertUserStatsAsync(Guid userId, double normalizedScore,
        int correctCount, int totalAnswered, int timeSpentSeconds, CancellationToken ct);
    Task UpsertCategoryStatsAsync(Guid userId, Guid categoryId,
        double normalizedScore, CancellationToken ct);
    Task UpsertDailyActivityAsync(Guid userId, DateOnly date,
        int correctAnswers, int timeSpentSeconds, CancellationToken ct);
}
