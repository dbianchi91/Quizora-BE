using Analytics.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Analytics.Infrastructure.Persistence.Configurations;

public class CategoryStatsConfiguration : IEntityTypeConfiguration<CategoryStats>
{
    public void Configure(EntityTypeBuilder<CategoryStats> builder)
    {
        builder.ToTable("CategoryStats", "analytics");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.UserId).IsRequired();
        builder.Property(e => e.CategoryId).IsRequired();
        builder.Property(e => e.AverageScore).HasColumnType("float");
        builder.Property(e => e.WeakAreaScore).HasColumnType("float");
        builder.Property(e => e.UpdatedAt).IsRequired();
        builder.HasIndex(e => new { e.UserId, e.CategoryId }).IsUnique();
    }
}
