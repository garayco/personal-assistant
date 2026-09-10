namespace PersonalAssistant.Api.Infrastructure.Database.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalAssistant.Api.Domain.Entities;

public class ChatSessionConfiguration : IEntityTypeConfiguration<ChatSession>
{
    public void Configure(EntityTypeBuilder<ChatSession> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Title).HasMaxLength(200).IsRequired();

        builder.HasMany(s => s.Messages)
               .WithOne(m => m.ChatSession)
               .HasForeignKey(m => m.ChatSessionId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
