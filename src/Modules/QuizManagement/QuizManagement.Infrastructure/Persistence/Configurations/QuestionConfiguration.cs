using Identity.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizManagement.Domain.Entities;
using QuizManagement.Domain.ValueObjects;

namespace QuizManagement.Infrastructure.Persistence.Configurations;

internal sealed class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable("Questions", "quiz");
        builder.HasKey(q => q.Id);
        builder.Property(q => q.Id).HasConversion(id => id.Value, v => QuestionId.From(v));
        builder.Property(q => q.CreatorId).HasConversion(id => id.Value, v => UserId.From(v));
        builder.Property(q => q.Text).HasMaxLength(2000).IsRequired();
        builder.Property(q => q.Explanation).HasMaxLength(2000);
        builder.Property(q => q.Difficulty).HasConversion<string>().HasMaxLength(20);

        builder.HasMany(q => q.Options).WithOne()
            .HasForeignKey("QuestionId").OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(q => q.Options).AutoInclude();

        builder.Ignore(q => q.DomainEvents);
    }
}

internal sealed class QuestionOptionConfiguration : IEntityTypeConfiguration<QuestionOption>
{
    public void Configure(EntityTypeBuilder<QuestionOption> builder)
    {
        builder.ToTable("QuestionOptions", "quiz");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).HasConversion(id => id.Value, v => QuestionOptionId.From(v));
        builder.Property(o => o.Text).HasMaxLength(500).IsRequired();
        builder.Property(o => o.IsCorrect).HasDefaultValue(false);
        builder.Property(o => o.OrderIndex).HasDefaultValue(0);
        builder.Ignore(o => o.DomainEvents);
    }
}
