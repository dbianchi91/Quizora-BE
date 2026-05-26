using Analytics.Application.DTOs;
using Analytics.Application.Interfaces;
using Dapper;
using Quizora.SharedKernel;
using StackExchange.Redis;

namespace Analytics.Infrastructure.Queries;

public sealed class AnalyticsDapperQueries(
    IDbConnectionFactory db,
    IConnectionMultiplexer redis) : IAnalyticsDapperQueries
{
    public async Task<UserStatsDto?> GetUserStatsAsync(Guid userId, CancellationToken ct)
    {
        using var conn = db.CreateConnection();
        return await conn.QueryFirstOrDefaultAsync<UserStatsDto>("""
            SELECT TotalExams, TotalCorrect, TotalAnswered, AverageScore,
                   BestScore, TotalTimeSpentSeconds, UpdatedAt
            FROM analytics.UserStats
            WHERE UserId = @UserId
            """, new { UserId = userId });
    }

    public async Task<IReadOnlyList<CategoryStatsDto>> GetCategoryStatsAsync(
        Guid userId, CancellationToken ct)
    {
        using var conn = db.CreateConnection();
        var rows = await conn.QueryAsync<CategoryStatsDto>("""
            SELECT cs.CategoryId, ISNULL(c.Name, 'Unknown') AS CategoryName,
                   cs.TotalExams, cs.AverageScore, cs.WeakAreaScore
            FROM analytics.CategoryStats cs
            LEFT JOIN quiz.Categories c ON c.Id = cs.CategoryId
            WHERE cs.UserId = @UserId
            ORDER BY cs.TotalExams DESC
            """, new { UserId = userId });
        return rows.ToList().AsReadOnly();
    }

    public async Task<PagedResult<ExamHistoryDto>> GetExamHistoryAsync(
        Guid userId, int page, int pageSize, CancellationToken ct)
    {
        pageSize = Math.Min(pageSize, 50);
        var offset = (page - 1) * pageSize;
        using var conn = db.CreateConnection();

        var rows = await conn.QueryAsync<ExamHistoryDto>("""
            SELECT e.Id AS SessionId, e.QuizTitle,
                   e.Score, e.NormalizedScore, e.CorrectCount, e.WrongCount, e.SkippedCount,
                   e.Type AS SessionType, e.CompletedAt
            FROM exam.ExamSessions e
            WHERE e.UserId = @UserId AND e.Status = 'Completed'
            ORDER BY e.CompletedAt DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """, new { UserId = userId, Offset = offset, PageSize = pageSize });

        var total = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM exam.ExamSessions WHERE UserId = @UserId AND Status = 'Completed'",
            new { UserId = userId });

        return new PagedResult<ExamHistoryDto>(rows.ToList().AsReadOnly(), total, page, pageSize);
    }

    public async Task<PagedResult<LeaderboardEntryDto>> GetLeaderboardAsync(
        Guid? categoryId, int page, int pageSize, CancellationToken ct)
    {
        pageSize = Math.Min(pageSize, 100);
        var offset = (page - 1) * pageSize;
        var redisDb = redis.GetDatabase();
        var key = categoryId.HasValue ? $"leaderboard:category:{categoryId.Value}" : "leaderboard:global";

        var total = (int)await redisDb.SortedSetLengthAsync(key);
        var entries = await redisDb.SortedSetRangeByRankWithScoresAsync(
            key, offset, offset + pageSize - 1, Order.Descending);

        if (entries.Length == 0)
            return new PagedResult<LeaderboardEntryDto>([], total, page, pageSize);

        var userIds = entries.Select(e => Guid.Parse(e.Element!)).ToArray();
        using var conn = db.CreateConnection();

        var users = (await conn.QueryAsync<(Guid Id, string UserName)>(
            "SELECT Id, UserName FROM [identity].Users WHERE Id IN @Ids",
            new { Ids = userIds })).ToDictionary(u => u.Id, u => u.UserName);

        var statsMap = (await conn.QueryAsync<(Guid UserId, int TotalExams)>(
            "SELECT UserId, TotalExams FROM analytics.UserStats WHERE UserId IN @Ids",
            new { Ids = userIds })).ToDictionary(s => s.UserId, s => s.TotalExams);

        var dtos = entries.Select((e, i) =>
        {
            var uid = Guid.Parse(e.Element!);
            return new LeaderboardEntryDto(
                offset + i + 1,
                uid,
                users.GetValueOrDefault(uid, "Unknown"),
                e.Score,
                statsMap.GetValueOrDefault(uid, 0));
        }).ToList().AsReadOnly();

        return new PagedResult<LeaderboardEntryDto>(dtos, total, page, pageSize);
    }

    public async Task<AdminOverviewDto> GetAdminOverviewAsync(CancellationToken ct)
    {
        using var conn = db.CreateConnection();
        return await conn.QueryFirstAsync<AdminOverviewDto>("""
            SELECT
                (SELECT COUNT(*) FROM analytics.UserStats WHERE TotalExams > 0) AS TotalActiveUsers,
                ISNULL((SELECT SUM(TotalExams) FROM analytics.UserStats), 0) AS TotalExamsAllTime,
                ISNULL((SELECT SUM(ExamsCount) FROM analytics.DailyActivity WHERE Date = @Today), 0) AS ExamsToday,
                ISNULL((SELECT AVG(AverageScore) FROM analytics.UserStats WHERE TotalExams > 0), 0.0) AS GlobalAverageScore
            """, new { Today = DateTime.UtcNow.Date });
    }
}
