using Analytics.Application.DTOs;
using Quizora.SharedKernel;

namespace Analytics.Application.Interfaces;

public interface IAnalyticsDapperQueries
{
    Task<UserStatsDto?> GetUserStatsAsync(Guid userId, CancellationToken ct);
    Task<IReadOnlyList<CategoryStatsDto>> GetCategoryStatsAsync(Guid userId, CancellationToken ct);
    Task<PagedResult<ExamHistoryDto>> GetExamHistoryAsync(Guid userId, int page, int pageSize, CancellationToken ct);
    Task<PagedResult<LeaderboardEntryDto>> GetLeaderboardAsync(Guid? categoryId, int page, int pageSize, CancellationToken ct);
    Task<AdminOverviewDto> GetAdminOverviewAsync(CancellationToken ct);
}
