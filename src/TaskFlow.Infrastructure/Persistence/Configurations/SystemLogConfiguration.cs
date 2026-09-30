using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

public class SystemLogConfiguration : IEntityTypeConfiguration<SystemLog>
{
    public void Configure(EntityTypeBuilder<SystemLog> builder)
    {
        builder.Property(l => l.Level)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(l => l.Message)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(l => l.ExceptionDetails)
            .HasColumnType("nvarchar(max)");

        builder.Property(l => l.Source)
            .HasMaxLength(200);

        builder.HasIndex(l => l.Level);
        builder.HasIndex(l => l.CreatedAt);
    }
}
