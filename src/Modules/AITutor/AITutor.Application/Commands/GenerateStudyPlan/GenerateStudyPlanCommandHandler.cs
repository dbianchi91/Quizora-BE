using AITutor.Application.Interfaces;
using AITutor.Application.Services;
using AITutor.Domain.Entities;
using Identity.Domain.ValueObjects;
using MediatR;
using Quizora.SharedKernel;

namespace AITutor.Application.Commands.GenerateStudyPlan;

public sealed class GenerateStudyPlanCommandHandler(
    IAIProvider ai, IAITutorRepository repo, IUserContextBuilder contextBuilder)
    : IRequestHandler<GenerateStudyPlanCommand, Result>
{
    public async Task<Result> Handle(GenerateStudyPlanCommand request, CancellationToken ct)
    {
        var userId = UserId.From(request.UserId);
        var systemPrompt = await contextBuilder.BuildSystemPromptAsync(request.UserId, null, ct);

        var planPrompt = new List<ChatMessageDto>
        {
            new("user", """
                Basandoti sul mio profilo, genera un piano di studio strutturato in JSON con questo formato:
                {
                  "priorityAreas": ["area1", "area2"],
                  "suggestedQuizIds": [],
                  "weeklyGoals": ["obiettivo1"],
                  "tips": ["consiglio1"]
                }
                Rispondi SOLO con il JSON, senza markdown o testo aggiuntivo.
                """)
        };

        var sb = new System.Text.StringBuilder();
        await foreach (var chunk in ai.StreamChatAsync(planPrompt, systemPrompt, ct))
            sb.Append(chunk);

        var contentJson = sb.ToString().Trim();

        var existingPlan = await repo.GetStudyPlanAsync(userId, ct);
        if (existingPlan is null)
            await repo.AddOrUpdateStudyPlanAsync(StudyPlan.Create(userId, contentJson, request.Automatic), ct);
        else
            existingPlan.Update(contentJson, request.Automatic);

        await repo.SaveChangesAsync(ct);
        return Result.Success();
    }
}
