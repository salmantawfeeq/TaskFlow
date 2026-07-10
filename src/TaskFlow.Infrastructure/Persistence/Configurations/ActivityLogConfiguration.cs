using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.Property(a => a.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(a => a.ActionType)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(a => a.RelatedTo)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(a => a.MetadataJson)
            .HasColumnType("nvarchar(max)");

        builder.HasOne(a => a.Project)
            .WithMany(p => p.Activities)
            .HasForeignKey(a => a.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.TaskItem)
            .WithMany(t => t.Activities)
            .HasForeignKey(a => a.TaskItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.User)
            .WithMany(u => u.Activities)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict); // audit trail must survive even if the user record changes

        // Activity timelines are always queried "most recent first" for a
        // given task/project, so index the FK + CreatedAt combination.
        builder.HasIndex(a => new { a.TaskItemId, a.CreatedAt });
        builder.HasIndex(a => new { a.ProjectId, a.CreatedAt });
    }
}
