using Analytics.Application.DTOs;
using Analytics.Application.Interfaces;
using MediatR;
using Quizora.SharedKernel;

namespace Analytics.Application.Queries.GetLeaderboard;

public sealed record GetLeaderboardQuery(Guid? CategoryId, int Page, int PageSize)
    : IRequest<Result<PagedResult<LeaderboardEntryDto>>>;

public sealed class GetLeaderboardQueryHandler(IAnalyticsDapperQueries queries)
    : IRequestHandler<GetLeaderboardQuery, Result<PagedResult<LeaderboardEntryDto>>>
{
    public async Task<Result<PagedResult<LeaderboardEntryDto>>> Handle(
        GetLeaderboardQuery request, CancellationToken ct)
    {
        var result = await queries.GetLeaderboardAsync(
            request.CategoryId, request.Page, request.PageSize, ct);
        return Result.Success(result);
    }
}
