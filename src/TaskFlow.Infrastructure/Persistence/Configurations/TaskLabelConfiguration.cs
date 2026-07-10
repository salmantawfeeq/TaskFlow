using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

public class TaskLabelConfiguration : IEntityTypeConfiguration<TaskLabel>
{
    public void Configure(EntityTypeBuilder<TaskLabel> builder)
    {
        // Pure join table with no surrogate key of its own - composite PK
        // of the two foreign keys is the natural and correct choice here.
        builder.HasKey(tl => new { tl.TaskItemId, tl.LabelId });

        builder.HasOne(tl => tl.TaskItem)
            .WithMany(t => t.TaskLabels)
            .HasForeignKey(tl => tl.TaskItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(tl => tl.Label)
            .WithMany(l => l.TaskLabels)
            .HasForeignKey(tl => tl.LabelId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
