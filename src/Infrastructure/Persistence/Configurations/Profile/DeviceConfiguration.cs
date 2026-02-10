using Bookazone.Domain.Entities.Profile;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Profile;

public class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ToTable("Devices", "profile");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Slug).HasMaxLength(300);
        builder.Property(e => e.Name).HasMaxLength(1000);
        builder.Property(e => e.Identifier).HasMaxLength(1000);
        builder.Property(e => e.NotificationToken).HasMaxLength(3000);
        builder.Property(e => e.Version).HasMaxLength(150);
        builder.Property(e => e.AppVersion).HasMaxLength(100);
        builder.Property(e => e.System).HasMaxLength(1000);
        builder.Property(e => e.Os).HasMaxLength(50);
        builder.Property(e => e.BiometricKey).HasMaxLength(5000);
        builder.Property(e => e.OtpEmailReference).HasMaxLength(100);
        builder.Property(e => e.OtpPhoneReference).HasMaxLength(100);
        builder.Property(e => e.MemorySlug).HasMaxLength(1000);
        builder.Property(e => e.CurrentSessionToken).HasMaxLength(15000);
        builder.HasOne(e => e.FkUser).WithMany(u => u.Devices).HasForeignKey(e => e.FkUserId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(e => e.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Active).HasDefaultValue(true);
        builder.Property(e => e.Deleted).HasDefaultValue(false);
    }
}
