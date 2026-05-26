using Microsoft.EntityFrameworkCore;
using QuizManagement.Application.Interfaces;
using QuizManagement.Domain.Entities;
using QuizManagement.Domain.ValueObjects;
using QuizManagement.Infrastructure.Persistence;

namespace QuizManagement.Infrastructure.Repositories;

internal sealed class QuizManagementRepository(QuizManagementDbContext db) : IQuizManagementRepository
{
    public Task<Quiz?> GetByIdAsync(QuizId id, CancellationToken ct) =>
        db.Quizzes.Include(q => q.Questions).FirstOrDefaultAsync(q => q.Id == id, ct);

    public Task<Question?> GetQuestionByIdAsync(QuestionId id, CancellationToken ct) =>
        db.Questions.Include(q => q.Options).FirstOrDefaultAsync(q => q.Id == id, ct);

    public Task<bool> SlugExistsAsync(string slug, CancellationToken ct) =>
        db.Quizzes.AnyAsync(q => q.Slug == slug, ct);

    public async Task AddQuizAsync(Quiz quiz, CancellationToken ct) =>
        await db.Quizzes.AddAsync(quiz, ct);

    public async Task AddQuestionAsync(Question question, CancellationToken ct) =>
        await db.Questions.AddAsync(question, ct);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
