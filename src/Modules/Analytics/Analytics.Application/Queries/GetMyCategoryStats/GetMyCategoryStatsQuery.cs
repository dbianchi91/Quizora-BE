using Analytics.Application.DTOs;
using Analytics.Application.Interfaces;
using MediatR;
using Quizora.SharedKernel;

namespace Analytics.Application.Queries.GetMyCategoryStats;

public sealed record GetMyCategoryStatsQuery(Guid UserId) : IRequest<Result<IReadOnlyList<CategoryStatsDto>>>;

public sealed class GetMyCategoryStatsQueryHandler(IAnalyticsDapperQueries queries)
    : IRequestHandler<GetMyCategoryStatsQuery, Result<IReadOnlyList<CategoryStatsDto>>>
{
    public async Task<Result<IReadOnlyList<CategoryStatsDto>>> Handle(
        GetMyCategoryStatsQuery request, CancellationToken ct)
    {
        var stats = await queries.GetCategoryStatsAsync(request.UserId, ct);
        return Result.Success(stats);
    }
}
