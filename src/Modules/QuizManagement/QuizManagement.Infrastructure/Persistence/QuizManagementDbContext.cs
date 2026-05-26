using Microsoft.EntityFrameworkCore;
using QuizManagement.Domain.Entities;

namespace QuizManagement.Infrastructure.Persistence;

public sealed class QuizManagementDbContext(DbContextOptions<QuizManagementDbContext> options)
    : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<Tag> Tags => Set<Tag>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(QuizManagementDbContext).Assembly);
        base.OnModelCreating(builder);
    }
}
