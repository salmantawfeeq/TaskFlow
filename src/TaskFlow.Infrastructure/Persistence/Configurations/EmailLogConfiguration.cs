using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

public class EmailLogConfiguration : IEntityTypeConfiguration<EmailLog>
{
    public void Configure(EntityTypeBuilder<EmailLog> builder)
    {
        builder.Property(e => e.ToEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.Subject)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(e => e.Body)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.EmailType)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(e => e.ToEmail);
        builder.HasIndex(e => e.SentAt);
    }
}
