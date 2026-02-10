using Bookazone.Domain.Entities.Permissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Permission;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRole");

        builder.HasKey(ur => new { ur.FkUserId, ur.FkRoleId });

        builder.HasOne(ur => ur.FkUser)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.FkUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ur => ur.FkRole)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.FkRoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}