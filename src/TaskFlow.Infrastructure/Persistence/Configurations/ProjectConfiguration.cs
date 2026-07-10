using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(2000);

        builder.Property(p => p.ColorHex)
            .IsRequired()
            .HasMaxLength(7);

        builder.Property(p => p.Status)
            .HasConversion<string>()   // store enum as readable string ("Active") not int, for easier SQL querying/reporting
            .HasMaxLength(30);

        builder.HasOne(p => p.Owner)
            .WithMany(u => u.OwnedProjects)
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Restrict); // never cascade-delete a user's identity because they own projects

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Projects)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.Team)
            .WithMany(t => t.Projects)
            .HasForeignKey(p => p.TeamId)
            .OnDelete(DeleteBehavior.SetNull);

        // Computed/derived properties must never be mapped to columns.
        builder.Ignore(p => p.ProgressPercentage);

        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.OwnerId);
        builder.HasIndex(p => p.IsArchived);
        builder.HasIndex(p => p.Name); // supports the Projects search feature
    }
}
