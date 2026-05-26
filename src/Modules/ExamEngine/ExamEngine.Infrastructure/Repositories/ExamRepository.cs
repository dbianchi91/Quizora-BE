using ExamEngine.Application.Interfaces;
using ExamEngine.Domain.Entities;
using ExamEngine.Domain.Enums;
using ExamEngine.Domain.ValueObjects;
using ExamEngine.Infrastructure.Persistence;
using Identity.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace ExamEngine.Infrastructure.Repositories;

internal sealed class ExamRepository(ExamEngineDbContext db) : IExamRepository
{
    public Task<ExamSession?> GetByIdAsync(ExamSessionId id, CancellationToken ct) =>
        db.ExamSessions.Include(s => s.Answers)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<ExamSession?> GetActiveOfficialSessionAsync(Guid quizId, UserId userId, CancellationToken ct) =>
        db.ExamSessions.FirstOrDefaultAsync(s =>
            s.QuizId == quizId && s.UserId == userId &&
            s.Type == SessionType.Official && s.Status == SessionStatus.InProgress, ct);

    public async Task AddAsync(ExamSession session, CancellationToken ct) =>
        await db.ExamSessions.AddAsync(session, ct);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
