using Analytics.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Analytics.Infrastructure.Persistence.Configurations;

public class UserStatsConfiguration : IEntityTypeConfiguration<UserStats>
{
    public void Configure(EntityTypeBuilder<UserStats> builder)
    {
        builder.ToTable("UserStats", "analytics");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.UserId).IsRequired();
        builder.Property(e => e.AverageScore).HasColumnType("float");
        builder.Property(e => e.BestScore).HasColumnType("float");
        builder.Property(e => e.UpdatedAt).IsRequired();
        builder.HasIndex(e => e.UserId).IsUnique();
    }
}
