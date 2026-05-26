using AITutor.Domain.Entities;
using AITutor.Domain.ValueObjects;
using Identity.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AITutor.Infrastructure.Persistence.Configurations;

internal sealed class StudyPlanConfiguration : IEntityTypeConfiguration<StudyPlan>
{
    public void Configure(EntityTypeBuilder<StudyPlan> builder)
    {
        builder.ToTable("StudyPlans", "ai");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasConversion(id => id.Value, v => StudyPlanId.From(v));
        builder.Property(p => p.UserId).HasConversion(id => id.Value, v => UserId.From(v));
        builder.HasIndex(p => p.UserId).IsUnique();
        builder.Property(p => p.ContentJson).IsRequired();
        builder.Ignore(p => p.DomainEvents);
    }
}
