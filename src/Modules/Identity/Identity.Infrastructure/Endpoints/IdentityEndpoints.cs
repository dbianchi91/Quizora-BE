using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Identity.Application.Commands.AssignCreatorRole;
using Identity.Application.Commands.Login;
using Identity.Application.Commands.Logout;
using Identity.Application.Commands.RefreshToken;
using Identity.Application.Commands.Register;
using Identity.Application.Queries.GetMe;
using Identity.Application.Queries.GetUsers;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Quizora.SharedKernel;

namespace Identity.Infrastructure.Endpoints;

internal sealed class IdentityEndpoints : IModuleEndpoints
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        MapAuth(app);
        MapUsers(app);
        MapAdmin(app);
    }

    private static void MapAuth(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth").WithTags("Auth");

        group.MapPost("/register", async (RegisterCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : HttpErrorMapper.ToHttpResult(result.Error);
        });

        group.MapPost("/login", async (LoginCommand command, ISender sender, HttpContext ctx) =>
        {
            var ip = ctx.Connection.RemoteIpAddress?.ToString();
            var result = await sender.Send(command with { IpAddress = ip });
            return result.IsSuccess ? Results.Ok(result.Value) : HttpErrorMapper.ToHttpResult(result.Error);
        });

        group.MapPost("/refresh", async ([FromBody] RefreshTokenRequest body, ISender sender, HttpContext ctx) =>
        {
            var ip = ctx.Connection.RemoteIpAddress?.ToString();
            var result = await sender.Send(new RefreshTokenCommand(body.Token, ip));
            return result.IsSuccess ? Results.Ok(result.Value) : HttpErrorMapper.ToHttpResult(result.Error);
        });

        group.MapPost("/logout", async ([FromBody] LogoutRequest body, ISender sender) =>
        {
            var result = await sender.Send(new LogoutCommand(body.RefreshToken));
            return result.IsSuccess ? Results.NoContent() : HttpErrorMapper.ToHttpResult(result.Error);
        }).RequireAuthorization();
    }

    private static void MapUsers(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/users").WithTags("Users").RequireAuthorization();

        group.MapGet("/me", async (ISender sender, ClaimsPrincipal user) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var result = await sender.Send(new GetMeQuery(userId));
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.NotFound(new { error = result.Error.Description });
        });
    }

    private static void MapAdmin(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/admin").WithTags("Admin").RequireAuthorization("Admin");

        group.MapGet("/users", async (ISender sender, int page = 1, int pageSize = 20) =>
            Results.Ok(await sender.Send(new GetUsersQuery(page, pageSize))));

        group.MapPost("/users/{userId:guid}/grant-creator", async (Guid userId, ISender sender) =>
        {
            var result = await sender.Send(new AssignCreatorRoleCommand(userId, Grant: true));
            return result.IsSuccess ? Results.NoContent() : Results.Problem(detail: result.Error.Description, statusCode: 404);
        });

        group.MapPost("/users/{userId:guid}/revoke-creator", async (Guid userId, ISender sender) =>
        {
            var result = await sender.Send(new AssignCreatorRoleCommand(userId, Grant: false));
            return result.IsSuccess ? Results.NoContent() : Results.Problem(detail: result.Error.Description, statusCode: 404);
        });
    }
}

internal record RefreshTokenRequest(string Token);
internal record LogoutRequest(string RefreshToken);
