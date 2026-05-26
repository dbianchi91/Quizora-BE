using AITutor.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AITutor.Infrastructure.Persistence;

public sealed class AITutorDbContext(DbContextOptions<AITutorDbContext> options) : DbContext(options)
{
    public DbSet<ChatSession> ChatSessions => Set<ChatSession>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<StudyPlan> StudyPlans => Set<StudyPlan>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(AITutorDbContext).Assembly);
        base.OnModelCreating(builder);
    }
}
