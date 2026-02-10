using Bookazone.Domain.Entities.Permissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Permission;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermission", "profile");
        builder.HasKey(rp => new { rp.FkRoleId, rp.FkPermissionId });
        builder.HasOne(rp => rp.FkRole).WithMany(r => r.RolePermissions).HasForeignKey(rp => rp.FkRoleId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(rp => rp.FkPermission).WithMany().HasForeignKey(rp => rp.FkPermissionId).OnDelete(DeleteBehavior.Cascade);
    }
}
