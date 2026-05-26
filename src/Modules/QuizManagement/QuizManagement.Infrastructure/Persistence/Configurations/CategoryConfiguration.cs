using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizManagement.Domain.Entities;
using QuizManagement.Domain.ValueObjects;

namespace QuizManagement.Infrastructure.Persistence.Configurations;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories", "quiz");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasConversion(id => id.Value, v => CategoryId.From(v));
        builder.Property(c => c.ParentId)
            .HasConversion(id => id == null ? (Guid?)null : id.Value, v => v == null ? null : CategoryId.From(v!.Value));
        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Slug).HasMaxLength(100).IsRequired();
        builder.HasIndex(c => c.Slug).IsUnique();
        builder.Property(c => c.OrderIndex).HasDefaultValue(0);
        builder.Ignore(c => c.DomainEvents);
    }
}
