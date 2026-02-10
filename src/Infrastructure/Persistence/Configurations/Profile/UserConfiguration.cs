using Bookazone.Domain.Entities.Profile;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Profile;

public class UserConfiguration : IEntityTypeConfiguration<Users>
{
    public void Configure(EntityTypeBuilder<Users> builder)
    {
        builder.ToTable("Users", "profile");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Firstname).HasMaxLength(50);
        builder.Property(u => u.Lastname).HasMaxLength(50);
        builder.Property(u => u.Username).HasMaxLength(150).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(50);
        builder.Property(u => u.Phone).HasMaxLength(50);
        builder.Property(u => u.Address).HasMaxLength(300);
        builder.Property(u => u.Gender).HasMaxLength(15);
        builder.Property(u => u.ProfileImage).HasMaxLength(2500);
        builder.Property(u => u.FcmToken).HasMaxLength(2000);
        builder.HasIndex(u => u.Username).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.Phone).IsUnique();
        builder.HasMany(u => u.Passwords).WithOne(p => p.FkUser).HasForeignKey(p => p.FkUserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(u => u.UserPermissions).WithOne(up => up.FkUser).HasForeignKey(up => up.FkUserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(u => u.Devices).WithOne(d => d.FkUser).HasForeignKey(d => d.FkUserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(u => u.AuthProviders).WithOne(e => e.FkUser).HasForeignKey(e => e.FkUserId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(u => u.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(u => u.Active).HasDefaultValue(true);
        builder.Property(u => u.Deleted).HasDefaultValue(false);
    }
}