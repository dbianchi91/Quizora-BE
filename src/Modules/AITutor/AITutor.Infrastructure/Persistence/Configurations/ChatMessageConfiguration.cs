using AITutor.Domain.Entities;
using AITutor.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AITutor.Infrastructure.Persistence.Configurations;

internal sealed class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("ChatMessages", "ai");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasConversion(id => id.Value, v => ChatMessageId.From(v));
        builder.Property(m => m.SessionId).HasConversion(id => id.Value, v => ChatSessionId.From(v));
        builder.Property(m => m.Role).HasMaxLength(20).IsRequired();
        builder.Property(m => m.Content).IsRequired();
        builder.Ignore(m => m.DomainEvents);
    }
}
