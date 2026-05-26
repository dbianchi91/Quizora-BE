using Analytics.Application.Interfaces;
using Analytics.Infrastructure.Persistence;
using Analytics.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Infrastructure.Repositories;

public sealed class AnalyticsRepository(AnalyticsDbContext context) : IAnalyticsRepository
{
    public async Task UpsertUserStatsAsync(Guid userId, double normalizedScore,
        int correctCount, int totalAnswered, int timeSpentSeconds, CancellationToken ct)
    {
        var stats = await context.UserStats.FirstOrDefaultAsync(s => s.UserId == userId, ct);
        if (stats is null)
        {
            stats = new UserStats { Id = Guid.NewGuid(), UserId = userId };
            context.UserStats.Add(stats);
        }

        if (normalizedScore > stats.BestScore) stats.BestScore = normalizedScore;
        stats.AverageScore = stats.TotalExams > 0
            ? (stats.AverageScore * stats.TotalExams + normalizedScore) / (stats.TotalExams + 1)
            : normalizedScore;
        stats.TotalExams++;
        stats.TotalCorrect += correctCount;
        stats.TotalAnswered += totalAnswered;
        stats.TotalTimeSpentSeconds += timeSpentSeconds;
        stats.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);
    }

    public async Task UpsertCategoryStatsAsync(Guid userId, Guid categoryId,
        double normalizedScore, CancellationToken ct)
    {
        var cat = await context.CategoryStats
            .FirstOrDefaultAsync(s => s.UserId == userId && s.CategoryId == categoryId, ct);
        if (cat is null)
        {
            cat = new CategoryStats { Id = Guid.NewGuid(), UserId = userId, CategoryId = categoryId };
            context.CategoryStats.Add(cat);
        }

        cat.AverageScore = cat.TotalExams > 0
            ? (cat.AverageScore * cat.TotalExams + normalizedScore) / (cat.TotalExams + 1)
            : normalizedScore;
        cat.TotalExams++;
        cat.WeakAreaScore = Math.Round(100 - cat.AverageScore, 2);
        cat.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(ct);
    }

    public async Task UpsertDailyActivityAsync(Guid userId, DateOnly date,
        int correctAnswers, int timeSpentSeconds, CancellationToken ct)
    {
        var activity = await context.DailyActivity
            .FirstOrDefaultAsync(a => a.UserId == userId && a.Date == date, ct);
        if (activity is null)
        {
            activity = new DailyActivity { Id = Guid.NewGuid(), UserId = userId, Date = date };
            context.DailyActivity.Add(activity);
        }

        activity.ExamsCount++;
        activity.CorrectAnswers += correctAnswers;
        activity.TimeSpentSeconds += timeSpentSeconds;

        await context.SaveChangesAsync(ct);
    }
}
