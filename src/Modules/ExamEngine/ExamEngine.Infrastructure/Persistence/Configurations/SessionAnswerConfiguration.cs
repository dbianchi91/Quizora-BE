using ExamEngine.Domain.Entities;
using ExamEngine.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ExamEngine.Infrastructure.Persistence.Configurations;

internal sealed class SessionAnswerConfiguration : IEntityTypeConfiguration<SessionAnswer>
{
    public void Configure(EntityTypeBuilder<SessionAnswer> builder)
    {
        builder.ToTable("SessionAnswers", "exam");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasConversion(id => id.Value, v => SessionAnswerId.From(v));
        builder.Property(a => a.SessionId).HasConversion(id => id.Value, v => ExamSessionId.From(v));
        builder.Property(a => a.QuestionId).IsRequired();
        builder.Property(a => a.SelectedOptionId).IsRequired(false);
        builder.Property(a => a.IsCorrect).HasDefaultValue(false);
        builder.Property(a => a.PointsAwarded).IsRequired();
        builder.Ignore(a => a.DomainEvents);
    }
}
