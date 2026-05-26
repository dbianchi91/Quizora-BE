using Analytics.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Infrastructure.Persistence;

public sealed class AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : DbContext(options)
{
    public DbSet<UserStats> UserStats => Set<UserStats>();
    public DbSet<CategoryStats> CategoryStats => Set<CategoryStats>();
    public DbSet<DailyActivity> DailyActivity => Set<DailyActivity>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(AnalyticsDbContext).Assembly);
        base.OnModelCreating(builder);
    }
}
