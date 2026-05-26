using Analytics.Application.DTOs;
using Analytics.Application.Interfaces;
using MediatR;
using Quizora.SharedKernel;

namespace Analytics.Application.Queries.GetMyStats;

public sealed record GetMyStatsQuery(Guid UserId) : IRequest<Result<UserStatsDto>>;

public sealed class GetMyStatsQueryHandler(IAnalyticsDapperQueries queries)
    : IRequestHandler<GetMyStatsQuery, Result<UserStatsDto>>
{
    public async Task<Result<UserStatsDto>> Handle(GetMyStatsQuery request, CancellationToken ct)
    {
        var stats = await queries.GetUserStatsAsync(request.UserId, ct);
        return Result.Success(stats ?? new UserStatsDto(0, 0, 0, 0, 0, 0, DateTime.UtcNow));
    }
}
