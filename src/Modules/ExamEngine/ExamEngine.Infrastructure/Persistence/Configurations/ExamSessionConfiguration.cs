using ExamEngine.Domain.Entities;
using ExamEngine.Domain.Enums;
using ExamEngine.Domain.ValueObjects;
using Identity.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExamEngine.Infrastructure.Persistence.Configurations;

internal sealed class ExamSessionConfiguration : IEntityTypeConfiguration<ExamSession>
{
    public void Configure(EntityTypeBuilder<ExamSession> builder)
    {
        builder.ToTable("ExamSessions", "exam");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasConversion(id => id.Value, v => ExamSessionId.From(v));
        builder.Property(s => s.UserId).HasConversion(id => id.Value, v => UserId.From(v));
        builder.Property(s => s.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.QuizId).IsRequired();
        builder.Property(s => s.Score).IsRequired(false);
        builder.Property(s => s.NormalizedScore).IsRequired(false);

        builder.OwnsOne(s => s.Config, cfg =>
        {
            cfg.Property(c => c.TimeLimitSeconds).HasColumnName("TimeLimitSeconds");
            cfg.Property(c => c.PointsCorrect).HasColumnName("PointsCorrect");
            cfg.Property(c => c.PointsWrong).HasColumnName("PointsWrong");
            cfg.Property(c => c.PointsSkipped).HasColumnName("PointsSkipped");
            cfg.Property(c => c.PassingScore).HasColumnName("PassingScore");
            cfg.Property(c => c.ShuffleQuestions).HasColumnName("ShuffleQuestions");
            cfg.Property(c => c.ShuffleOptions).HasColumnName("ShuffleOptions");
            cfg.Property(c => c.CategoryId).HasColumnName("CategoryId");
            cfg.Property(c => c.QuizTitle).HasColumnName("QuizTitle").HasMaxLength(200);
        });

        builder.HasMany(s => s.Answers).WithOne()
            .HasForeignKey(a => a.SessionId)
            .HasPrincipalKey(s => s.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(s => s.DomainEvents);
    }
}
