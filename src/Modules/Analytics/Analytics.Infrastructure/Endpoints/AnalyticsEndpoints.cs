using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Analytics.Application.Queries.GetAdminOverview;
using Analytics.Application.Queries.GetLeaderboard;
using Analytics.Application.Queries.GetMyCategoryStats;
using Analytics.Application.Queries.GetMyHistory;
using Analytics.Application.Queries.GetMyStats;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Quizora.SharedKernel;

namespace Analytics.Infrastructure.Endpoints;

internal sealed class AnalyticsEndpoints : IModuleEndpoints
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/analytics").WithTags("Analytics").RequireAuthorization();

        group.MapGet("/me", async (ISender sender, ClaimsPrincipal user) =>
        {
            var userId = GetUserId(user);
            var result = await sender.Send(new GetMyStatsQuery(userId));
            return result.IsSuccess ? Results.Ok(result.Value) : HttpErrorMapper.ToHttpResult(result.Error);
        });

        group.MapGet("/me/categories", async (ISender sender, ClaimsPrincipal user) =>
        {
            var userId = GetUserId(user);
            var result = await sender.Send(new GetMyCategoryStatsQuery(userId));
            return result.IsSuccess ? Results.Ok(result.Value) : HttpErrorMapper.ToHttpResult(result.Error);
        });

        group.MapGet("/me/history", async (ISender sender, ClaimsPrincipal user,
            int page = 1, int pageSize = 10) =>
        {
            var userId = GetUserId(user);
            var result = await sender.Send(new GetMyHistoryQuery(userId, page, pageSize));
            return result.IsSuccess ? Results.Ok(result.Value) : HttpErrorMapper.ToHttpResult(result.Error);
        });

        group.MapGet("/leaderboard", async (ISender sender, Guid? category,
            int page = 1, int pageSize = 20) =>
        {
            var result = await sender.Send(new GetLeaderboardQuery(category, page, pageSize));
            return result.IsSuccess ? Results.Ok(result.Value) : HttpErrorMapper.ToHttpResult(result.Error);
        }).AllowAnonymous();

        group.MapGet("/admin/overview", async (ISender sender) =>
        {
            var result = await sender.Send(new GetAdminOverviewQuery());
            return result.IsSuccess ? Results.Ok(result.Value) : HttpErrorMapper.ToHttpResult(result.Error);
        }).RequireAuthorization("Admin");
    }

    private static Guid GetUserId(ClaimsPrincipal user) =>
        Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
}
