using Identity.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizManagement.Domain.Entities;
using QuizManagement.Domain.ValueObjects;

namespace QuizManagement.Infrastructure.Persistence.Configurations;

internal sealed class QuizConfiguration : IEntityTypeConfiguration<Quiz>
{
    public void Configure(EntityTypeBuilder<Quiz> builder)
    {
        builder.ToTable("Quizzes", "quiz");
        builder.HasKey(q => q.Id);
        builder.Property(q => q.Id).HasConversion(id => id.Value, v => QuizId.From(v));
        builder.Property(q => q.CategoryId).HasConversion(id => id.Value, v => CategoryId.From(v));
        builder.Property(q => q.CreatorId).HasConversion(id => id.Value, v => UserId.From(v));
        builder.Property(q => q.Title).HasMaxLength(200).IsRequired();
        builder.Property(q => q.Slug).HasMaxLength(200).IsRequired();
        builder.HasIndex(q => q.Slug).IsUnique();
        builder.Property(q => q.Status).HasConversion<string>().HasMaxLength(20);

        builder.Property<bool>("IsDeleted").HasDefaultValue(false);
        builder.HasQueryFilter(q => !EF.Property<bool>(q, "IsDeleted"));

        // ExamConfig owned entity — columns stored in-table
        builder.OwnsOne(q => q.ExamConfig, cfg =>
        {
            cfg.Property(c => c.TimeLimitSeconds).HasColumnName("TimeLimitSeconds").IsRequired();
            cfg.Property(c => c.PointsCorrect).HasColumnName("PointsCorrect").IsRequired();
            cfg.Property(c => c.PointsWrong).HasColumnName("PointsWrong").IsRequired();
            cfg.Property(c => c.PointsSkipped).HasColumnName("PointsSkipped").IsRequired();
            cfg.Property(c => c.PassingScore).HasColumnName("PassingScore");
            cfg.Property(c => c.ShuffleQuestions).HasColumnName("ShuffleQuestions").IsRequired();
            cfg.Property(c => c.ShuffleOptions).HasColumnName("ShuffleOptions").IsRequired();
        });

        // QuizQuestions — owned collection (join table)
        builder.OwnsMany(q => q.Questions, qq =>
        {
            qq.ToTable("QuizQuestions", "quiz");
            qq.Property(x => x.QuizId).HasConversion(id => id.Value, v => QuizId.From(v));
            qq.Property(x => x.QuestionId).HasConversion(id => id.Value, v => QuestionId.From(v));
            qq.HasKey(x => new { x.QuizId, x.QuestionId });
        });

        builder.Ignore(q => q.Tags);
        builder.Ignore(q => q.DomainEvents);
    }
}
