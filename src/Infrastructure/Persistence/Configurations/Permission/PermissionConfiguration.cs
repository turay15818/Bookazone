using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Permission;

public class PermissionConfiguration : IEntityTypeConfiguration<Domain.Entities.Permissions.Permission>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Permissions.Permission> builder)
    {
        builder.ToTable("Permissions", "profile");
        builder.HasIndex(u => u.Name).IsUnique();
        builder.HasIndex(u => u.Slug).IsUnique();
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Active).IsRequired();
        builder.Property(p => p.Deleted).IsRequired();
        builder.Property(p => p.CreatedBy).HasMaxLength(50);
        builder.Property(p => p.UpdatedBy).HasMaxLength(50);
        builder.Property(p => p.DeletedBy).HasMaxLength(50);
        builder.Property(p => p.DeletedReason).HasMaxLength(500);
        builder.Property(p => p.DateCreated).IsRequired();
        builder.Property(p => p.DateUpdated);
        builder.Property(p => p.DateDeleted);
        builder.Property(p=> p.IsSystem).IsRequired();
        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Slug).IsRequired().HasMaxLength(150);
        builder.Property(p => p.Description).HasMaxLength(500);
    }
}