namespace PersonalAssistant.Api.Infrastructure.Database.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalAssistant.Api.Domain.Entities;

public class SessionMemoryConfiguration : IEntityTypeConfiguration<SessionMemory>
{
    public void Configure(EntityTypeBuilder<SessionMemory> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Content).IsRequired();
        builder.Property(m => m.Embedding).HasColumnType("vector(1024)").IsRequired();

        builder.HasOne(m => m.ChatSession)
               .WithMany()
               .HasForeignKey(m => m.ChatSessionId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
