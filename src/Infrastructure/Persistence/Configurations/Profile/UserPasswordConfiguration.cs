using Bookazone.Domain.Entities.Profile;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Profile;

public class UserPasswordConfiguration : IEntityTypeConfiguration<UserPassword>
{
    public void Configure(EntityTypeBuilder<UserPassword> builder)
    {
        builder.ToTable("UserPasswords", "profile");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Password).HasMaxLength(int.MaxValue).IsRequired();
        builder.Property(e => e.UpdatedBy).HasMaxLength(255);
        builder.HasOne(e => e.FkUser).WithMany(u => u.Passwords).HasForeignKey(e => e.FkUserId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(e => e.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Active).HasDefaultValue(true);
        builder.Property(e => e.Deleted).HasDefaultValue(false);
    }
}