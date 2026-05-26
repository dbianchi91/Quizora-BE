using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Quizora.SharedKernel;
using QuizManagement.Application.Commands.AddQuestionToQuiz;
using QuizManagement.Application.Commands.CreateQuestion;
using QuizManagement.Application.Commands.CreateQuiz;
using QuizManagement.Application.Commands.DeleteQuiz;
using QuizManagement.Application.Commands.PublishQuiz;
using QuizManagement.Application.Commands.UpdateQuiz;
using QuizManagement.Application.Queries.GetCategories;
using QuizManagement.Application.Queries.GetQuestionPool;
using QuizManagement.Application.Queries.GetQuizById;
using QuizManagement.Application.Queries.GetQuizzes;

namespace QuizManagement.Infrastructure.Endpoints;

internal sealed class QuizManagementEndpoints : IModuleEndpoints
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        MapCategories(app);
        MapQuizzes(app);
        MapQuestions(app);
    }

    private static void MapCategories(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/categories").WithTags("Categories");
        group.MapGet("/", async (ISender sender) =>
            Results.Ok(await sender.Send(new GetCategoriesQuery()))).RequireAuthorization();
    }

    private static void MapQuizzes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/quizzes").WithTags("Quizzes");

        group.MapGet("/", async (ISender sender, Guid? categoryId, string? search, int page = 1) =>
            Results.Ok(await sender.Send(new GetQuizzesQuery(categoryId, search, page))))
            .RequireAuthorization();

        group.MapGet("/{id:guid}", async (Guid id, ISender sender) =>
        {
            var result = await sender.Send(new GetQuizByIdQuery(id));
            return result is null ? Results.NotFound() : Results.Ok(result);
        }).RequireAuthorization();

        group.MapPost("/", async (CreateQuizCommand command, ISender sender, ClaimsPrincipal user) =>
        {
            var creatorId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var result = await sender.Send(command with { CreatorId = creatorId });
            return result.IsSuccess ? Results.Ok(result.Value) : HttpErrorMapper.ToHttpResult(result.Error);
        }).RequireAuthorization("Creator");

        group.MapPut("/{id:guid}", async (Guid id, UpdateQuizCommand command, ISender sender, ClaimsPrincipal user) =>
        {
            var creatorId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var result = await sender.Send(command with { QuizId = id, RequestingUserId = creatorId });
            return result.IsSuccess ? Results.NoContent() : HttpErrorMapper.ToHttpResult(result.Error);
        }).RequireAuthorization("Creator");

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender, ClaimsPrincipal user) =>
        {
            var creatorId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var result = await sender.Send(new DeleteQuizCommand(id, creatorId));
            return result.IsSuccess ? Results.NoContent() : HttpErrorMapper.ToHttpResult(result.Error);
        }).RequireAuthorization("Creator");

        group.MapPost("/{id:guid}/publish", async (Guid id, ISender sender, ClaimsPrincipal user) =>
        {
            var creatorId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var result = await sender.Send(new PublishQuizCommand(id, creatorId));
            return result.IsSuccess ? Results.NoContent() : HttpErrorMapper.ToHttpResult(result.Error);
        }).RequireAuthorization("Creator");

        group.MapPost("/{id:guid}/questions", async (Guid id, AddQuestionToQuizCommand cmd,
            ISender sender, ClaimsPrincipal user) =>
        {
            var creatorId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var result = await sender.Send(cmd with { QuizId = id, RequestingUserId = creatorId });
            return result.IsSuccess ? Results.NoContent() : HttpErrorMapper.ToHttpResult(result.Error);
        }).RequireAuthorization("Creator");
    }

    private static void MapQuestions(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/questions").WithTags("Questions");

        group.MapGet("/", async (ISender sender, ClaimsPrincipal user) =>
        {
            var creatorId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            return Results.Ok(await sender.Send(new GetQuestionPoolQuery(creatorId)));
        }).RequireAuthorization("Creator");

        group.MapPost("/", async (CreateQuestionCommand command, ISender sender, ClaimsPrincipal user) =>
        {
            var creatorId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var result = await sender.Send(command with { CreatorId = creatorId });
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Problem(detail: result.Error.Description, statusCode: 422);
        }).RequireAuthorization("Creator");
    }
}
