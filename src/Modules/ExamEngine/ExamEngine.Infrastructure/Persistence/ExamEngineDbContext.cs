using ExamEngine.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExamEngine.Infrastructure.Persistence;

public sealed class ExamEngineDbContext(DbContextOptions<ExamEngineDbContext> options)
    : DbContext(options)
{
    public DbSet<ExamSession> ExamSessions => Set<ExamSession>();
    public DbSet<SessionAnswer> SessionAnswers => Set<SessionAnswer>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(ExamEngineDbContext).Assembly);
        base.OnModelCreating(builder);
    }
}
