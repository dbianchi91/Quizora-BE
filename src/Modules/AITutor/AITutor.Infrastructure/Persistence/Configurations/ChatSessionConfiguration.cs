using AITutor.Domain.Entities;
using AITutor.Domain.ValueObjects;
using Identity.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AITutor.Infrastructure.Persistence.Configurations;

internal sealed class ChatSessionConfiguration : IEntityTypeConfiguration<ChatSession>
{
    public void Configure(EntityTypeBuilder<ChatSession> builder)
    {
        builder.ToTable("ChatSessions", "ai");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasConversion(id => id.Value, v => ChatSessionId.From(v));
        builder.Property(s => s.UserId).HasConversion(id => id.Value, v => UserId.From(v));
        builder.HasMany(s => s.Messages).WithOne()
            .HasForeignKey(m => m.SessionId)
            .HasPrincipalKey(s => s.Id)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Ignore(s => s.DomainEvents);
    }
}
