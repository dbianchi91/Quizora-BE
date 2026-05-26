using Analytics.Application.DTOs;
using Analytics.Application.Interfaces;
using MediatR;
using Quizora.SharedKernel;

namespace Analytics.Application.Queries.GetAdminOverview;

public sealed record GetAdminOverviewQuery : IRequest<Result<AdminOverviewDto>>;

public sealed class GetAdminOverviewQueryHandler(IAnalyticsDapperQueries queries)
    : IRequestHandler<GetAdminOverviewQuery, Result<AdminOverviewDto>>
{
    public async Task<Result<AdminOverviewDto>> Handle(
        GetAdminOverviewQuery request, CancellationToken ct)
    {
        var overview = await queries.GetAdminOverviewAsync(ct);
        return Result.Success(overview);
    }
}
