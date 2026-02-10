using Bookazone.Domain.Entities.Profile;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Profile;

public class OneTimePasswordConfiguration : IEntityTypeConfiguration<OneTimePassword>
{
    public void Configure(EntityTypeBuilder<OneTimePassword> builder)
    {
        builder.ToTable("OneTimePasswords", "profile");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Reference).HasMaxLength(100);
        builder.Property(e => e.Otp).HasMaxLength(50);
        builder.Property(e => e.Email).HasMaxLength(300);
        builder.Property(e => e.Phone).HasMaxLength(50);
        builder.Property(e => e.Purpose).HasMaxLength(300);
        builder.Property(e => e.TransmitStatus).HasMaxLength(300);
        builder.Property(e => e.ErrorCode).HasMaxLength(300);
        builder.Property(e => e.ErrorMessage).HasMaxLength(300);
        builder.Property(e => e.Token).HasMaxLength(5120);
        builder.Property(e => e.DeviceIdentifier).HasMaxLength(500);
        builder.Property(e => e.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Active).HasDefaultValue(true);
        builder.Property(e => e.Deleted).HasDefaultValue(false);
    }
}