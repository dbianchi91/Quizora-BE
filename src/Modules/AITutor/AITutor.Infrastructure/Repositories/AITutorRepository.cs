using AITutor.Application.Interfaces;
using AITutor.Domain.Entities;
using AITutor.Domain.ValueObjects;
using AITutor.Infrastructure.Persistence;
using Identity.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace AITutor.Infrastructure.Repositories;

internal sealed class AITutorRepository(AITutorDbContext db) : IAITutorRepository
{
    public Task<ChatSession?> GetSessionAsync(ChatSessionId id, CancellationToken ct) =>
        db.ChatSessions.Include(s => s.Messages.OrderBy(m => m.CreatedAt))
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<ChatSession?> GetLatestSessionForUserAsync(UserId userId, CancellationToken ct) =>
        db.ChatSessions.Include(s => s.Messages.OrderBy(m => m.CreatedAt))
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.LastMessageAt)
            .FirstOrDefaultAsync(ct);

    public Task<StudyPlan?> GetStudyPlanAsync(UserId userId, CancellationToken ct) =>
        db.StudyPlans.FirstOrDefaultAsync(p => p.UserId == userId, ct);

    public async Task AddSessionAsync(ChatSession session, CancellationToken ct) =>
        await db.ChatSessions.AddAsync(session, ct);

    public async Task AddOrUpdateStudyPlanAsync(StudyPlan plan, CancellationToken ct)
    {
        var existing = await db.StudyPlans.FirstOrDefaultAsync(
            p => p.UserId == plan.UserId, ct);
        if (existing is null) await db.StudyPlans.AddAsync(plan, ct);
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
