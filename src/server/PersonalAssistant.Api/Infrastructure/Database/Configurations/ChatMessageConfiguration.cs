namespace PersonalAssistant.Api.Infrastructure.Database.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalAssistant.Api.Domain.Entities;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.HasKey(m => m.Id);

        // Guarda el enum como string ("User", "Assistant") en lugar de int
        builder.Property(m => m.Role)
               .HasConversion<string>();
    }
}