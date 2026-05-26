using ExamEngine.Domain.Entities;
using ExamEngine.Domain.ValueObjects;
using Identity.Domain.ValueObjects;

namespace ExamEngine.Application.Interfaces;

public interface IExamRepository
{
    Task<ExamSession?> GetByIdAsync(ExamSessionId id, CancellationToken ct);
    Task<ExamSession?> GetActiveOfficialSessionAsync(Guid quizId, UserId userId, CancellationToken ct);
    Task AddAsync(ExamSession session, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
