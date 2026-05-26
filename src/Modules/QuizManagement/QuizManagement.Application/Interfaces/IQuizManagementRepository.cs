using QuizManagement.Domain.Entities;
using QuizManagement.Domain.ValueObjects;

namespace QuizManagement.Application.Interfaces;

public interface IQuizManagementRepository
{
    Task<Quiz?> GetByIdAsync(QuizId id, CancellationToken ct);
    Task<Question?> GetQuestionByIdAsync(QuestionId id, CancellationToken ct);
    Task<bool> SlugExistsAsync(string slug, CancellationToken ct);
    Task AddQuizAsync(Quiz quiz, CancellationToken ct);
    Task AddQuestionAsync(Question question, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
