using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskItemEntity = TaskFlow.Domain.Entities.TaskItem;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItemEntity>
{
    public void Configure(EntityTypeBuilder<TaskItemEntity> builder)
    {
        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(t => t.Description)
            .HasMaxLength(4000);

        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(t => t.Priority)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne(t => t.Project)
            .WithMany(p => p.Tasks)
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade); // deleting a project deletes its tasks

        builder.HasOne(t => t.ParentTask)
            .WithMany(t => t.SubTasks)
            .HasForeignKey(t => t.ParentTaskId)
            .OnDelete(DeleteBehavior.Restrict); // avoid multiple cascade paths / cycles on self-reference

        builder.HasOne(t => t.CreatedByUser)
            .WithMany(u => u.CreatedTasks)
            .HasForeignKey(t => t.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(t => t.IsOverdue);
        builder.Ignore(t => t.ChecklistProgressPercentage);

        builder.HasIndex(t => new { t.ProjectId, t.Status });
        builder.HasIndex(t => t.DueDate);
        builder.HasIndex(t => t.Priority);
        builder.HasIndex(t => t.IsDeleted);
    }
}
