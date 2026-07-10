using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(n => n.Message)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(n => n.LinkUrl)
            .HasMaxLength(500);

        builder.Property(n => n.Type)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.HasOne(n => n.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(n => n.RelatedTaskItem)
            .WithMany()
            .HasForeignKey(n => n.RelatedTaskItemId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(n => n.RelatedProject)
            .WithMany()
            .HasForeignKey(n => n.RelatedProjectId)
            .OnDelete(DeleteBehavior.SetNull);

        // Powers the notification bell: "unread notifications for this user, newest first".
        builder.HasIndex(n => new { n.UserId, n.IsRead, n.CreatedAt });
    }
}
