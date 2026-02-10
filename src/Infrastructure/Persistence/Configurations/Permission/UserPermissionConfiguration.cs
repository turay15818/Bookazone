using Bookazone.Domain.Entities.Permissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Permission;

public class UserPermissionConfiguration : IEntityTypeConfiguration<UserPermission>
{
    public void Configure(EntityTypeBuilder<UserPermission> builder)
    {
        builder.ToTable("UserPermission");
        builder.HasKey(up => new { up.FkUserId, up.FkPermissionId });
        builder.HasOne(up => up.FkUser).WithMany(u => u.UserPermissions).HasForeignKey(up => up.FkUserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(up => up.FkPermission).WithMany().HasForeignKey(up => up.FkPermissionId).OnDelete(DeleteBehavior.Cascade);
    }
}