using AITutor.Domain.Entities;
using AITutor.Domain.ValueObjects;
using Identity.Domain.ValueObjects;

namespace AITutor.Application.Interfaces;

public interface IAITutorRepository
{
    Task<ChatSession?> GetSessionAsync(ChatSessionId id, CancellationToken ct);
    Task<ChatSession?> GetLatestSessionForUserAsync(UserId userId, CancellationToken ct);
    Task<StudyPlan?> GetStudyPlanAsync(UserId userId, CancellationToken ct);
    Task AddSessionAsync(ChatSession session, CancellationToken ct);
    Task AddOrUpdateStudyPlanAsync(StudyPlan plan, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
