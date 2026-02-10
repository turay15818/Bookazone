using Bookazone.Domain.Entities.Permissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Permission;

public class PermissionGroupConfiguration : IEntityTypeConfiguration<PermissionGroup>
{
    public void Configure(EntityTypeBuilder<PermissionGroup> builder)
    {
        builder.ToTable("PermissionGroup", "profile");
        builder.HasIndex(u => u.Name).IsUnique();
        builder.HasKey(pg => pg.Id);
        builder.Property(pg => pg.Name).IsRequired().HasMaxLength(100);
        builder.Property(pg => pg.Description).HasMaxLength(250);
        
    }
}
