using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Persistence.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.JobTitle)
            .HasMaxLength(150);

        builder.Property(u => u.ProfileImagePath)
            .HasMaxLength(500);

        builder.Property(u => u.Bio)
            .HasMaxLength(1000);

        // FullName is computed in C# (first + last), never stored in the DB.
        builder.Ignore(u => u.FullName);

        builder.HasOne(u => u.Department)
            .WithMany(d => d.Members)
            .HasForeignKey(u => u.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull); // deleting a department must not cascade-delete its users

        // Helpful index: looking up active users, or users by department, is common
        // on the Team Management and Admin > Manage Users screens.
        builder.HasIndex(u => u.DepartmentId);
        builder.HasIndex(u => u.IsActive);
    }
}
