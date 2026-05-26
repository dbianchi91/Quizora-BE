using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ExamEngine.Application.Commands.AbandonExam;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ExamEngine.Application.Commands.AnswerQuestion;
using ExamEngine.Application.Commands.StartExam;
using ExamEngine.Application.Commands.SubmitExam;
using ExamEngine.Application.Queries.GetExamHistory;
using ExamEngine.Application.Queries.GetExamResults;
using ExamEngine.Application.Queries.GetExamState;
using ExamEngine.Application.Queries.GetSimulationHistory;
using MediatR;
using Quizora.SharedKernel;

namespace ExamEngine.Infrastructure.Endpoints;

internal sealed class ExamEngineEndpoints : IModuleEndpoints
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/exams").WithTags("Exams").RequireAuthorization();

        group.MapPost("/start", async (StartExamRequest req, ISender sender,
            ClaimsPrincipal user, HttpRequest httpRequest) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var idempotencyKey = httpRequest.Headers["Idempotency-Key"].FirstOrDefault()
                ?? Guid.NewGuid().ToString();
            var result = await sender.Send(
                new StartExamCommand(req.QuizId, userId, req.SessionType, idempotencyKey));
            return result.IsSuccess ? Results.Ok(new { SessionId = result.Value }) : HttpErrorMapper.ToHttpResult(result.Error);
        });

        group.MapGet("/{id:guid}/state", async (Guid id, ISender sender, ClaimsPrincipal user) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var result = await sender.Send(new GetExamStateQuery(id, userId));
            return result.IsSuccess ? Results.Ok(result.Value) : HttpErrorMapper.ToHttpResult(result.Error);
        });

        group.MapPost("/{id:guid}/answer", async (Guid id, AnswerRequest req,
            ISender sender, ClaimsPrincipal user) =>
        {
            var result = await sender.Send(
                new AnswerQuestionCommand(id, req.QuestionId, req.SelectedOptionId, req.TimeSpentSeconds));
            return result.IsSuccess ? Results.Ok(result.Value) : HttpErrorMapper.ToHttpResult(result.Error);
        });

        group.MapPost("/{id:guid}/submit", async (Guid id, ISender sender,
            ClaimsPrincipal user, HttpRequest httpRequest) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var idempotencyKey = httpRequest.Headers["Idempotency-Key"].FirstOrDefault()
                ?? Guid.NewGuid().ToString();
            var result = await sender.Send(new SubmitExamCommand(id, userId, idempotencyKey));
            return result.IsSuccess ? Results.NoContent() : HttpErrorMapper.ToHttpResult(result.Error);
        });

        group.MapPost("/{id:guid}/abandon", async (Guid id, ISender sender, ClaimsPrincipal user) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var result = await sender.Send(new AbandonExamCommand(id, userId));
            return result.IsSuccess ? Results.NoContent() : HttpErrorMapper.ToHttpResult(result.Error);
        });

        group.MapGet("/{id:guid}/results", async (Guid id, ISender sender, ClaimsPrincipal user) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var result = await sender.Send(new GetExamResultsQuery(id, userId));
            return result.IsSuccess ? Results.Ok(result.Value) : HttpErrorMapper.ToHttpResult(result.Error);
        });

        group.MapGet("/history", async (ISender sender, ClaimsPrincipal user) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            return Results.Ok(await sender.Send(new GetExamHistoryQuery(userId)));
        });

        group.MapGet("/simulations/{quizId:guid}", async (Guid quizId, ISender sender, ClaimsPrincipal user) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            return Results.Ok(await sender.Send(new GetSimulationHistoryQuery(quizId, userId)));
        });
    }
}

internal record StartExamRequest(Guid QuizId, string SessionType);
internal record AnswerRequest(Guid QuestionId, Guid? SelectedOptionId, int TimeSpentSeconds);
