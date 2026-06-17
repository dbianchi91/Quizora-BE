using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using AITutor.Application.Commands.GenerateStudyPlan;
using AITutor.Application.Interfaces;
using AITutor.Application.Queries.GetStudyPlan;
using AITutor.Application.Services;
using AITutor.Domain.Entities;
using AITutor.Domain.ValueObjects;
using Identity.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Quizora.SharedKernel;

namespace AITutor.Infrastructure.Endpoints;

internal sealed class AITutorEndpoints : IModuleEndpoints
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/ai").WithTags("AI").RequireAuthorization();

        group.MapPost("/chat/stream", async (
            ChatStreamRequest req,
            IAIProvider ai,
            IAITutorRepository repo,
            IUserContextBuilder contextBuilder,
            ClaimsPrincipal user,
            HttpResponse response,
            CancellationToken ct) =>
        {
            var userId = UserId.From(Guid.Parse(
                user.FindFirstValue(JwtRegisteredClaimNames.Sub)!));

            ChatSession session;
            bool isNewSession;
            if (req.SessionId.HasValue)
            {
                var existing = await repo.GetSessionAsync(
                    ChatSessionId.From(req.SessionId.Value), ct);
                isNewSession = existing is null;
                session = existing ?? ChatSession.Create(userId);
            }
            else
            {
                var latest = await repo.GetLatestSessionForUserAsync(userId, ct);
                isNewSession = latest is null;
                session = latest ?? ChatSession.Create(userId);
            }

            if (isNewSession)
                await repo.AddSessionAsync(session, ct);

            session.AddUserMessage(req.Message);

            var history = session.Messages
                .Select(m => new ChatMessageDto(m.Role, m.Content))
                .ToList();

            var systemPrompt = await contextBuilder.BuildSystemPromptAsync(
                userId.Value, req.PageContext, ct);

            response.ContentType = "text/event-stream";
            response.Headers.CacheControl = "no-cache";
            response.Headers.Connection = "keep-alive";

            var sb = new StringBuilder();
            await foreach (var chunk in ai.StreamChatAsync(history, systemPrompt, ct))
            {
                sb.Append(chunk);
                var payload = JsonSerializer.Serialize(new { chunk });
                await response.WriteAsync($"data: {payload}\n\n", ct);
                await response.Body.FlushAsync(ct);
            }

            await response.WriteAsync("data: [DONE]\n\n", ct);
            await response.Body.FlushAsync(ct);

            session.AddAssistantMessage(sb.ToString());
            await repo.SaveChangesAsync(ct);
        });

        group.MapGet("/study-plan", async (ISender sender, ClaimsPrincipal user) =>
        {
            var userId = Guid.Parse(
                user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var plan = await sender.Send(new GetStudyPlanQuery(userId));
            return plan is null ? Results.NotFound() : Results.Ok(plan);
        });

        group.MapPost("/study-plan/generate", async (ISender sender, ClaimsPrincipal user) =>
        {
            var userId = Guid.Parse(
                user.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            var result = await sender.Send(new GenerateStudyPlanCommand(userId, Automatic: false));
            return result.IsSuccess ? Results.Ok() : Results.Problem(detail: result.Error.Description, statusCode: 400);
        });
    }
}

internal record ChatStreamRequest(string Message, Guid? SessionId = null, string? PageContext = null);
