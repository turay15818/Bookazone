using Bookazone.Domain.Entities.Profile;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Profile;

public class AuthProviderConfiguration : IEntityTypeConfiguration<AuthProvider>
{
    public void Configure(EntityTypeBuilder<AuthProvider> builder)
    {
        builder.ToTable("AuthProviders", "profile");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Provider).HasMaxLength(50);
        builder.Property(e => e.ProviderUserId).HasMaxLength(200);
        builder.Property(e => e.Email).HasMaxLength(500);
        builder.HasOne(e => e.FkUser).WithMany(u => u.AuthProviders).HasForeignKey(e => e.FkUserId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(e => e.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Active).HasDefaultValue(true);
        builder.Property(e => e.Deleted).HasDefaultValue(false);
        builder.Property(x => x.PictureUrl).HasMaxLength(2000);
        builder.HasIndex(x => new { x.Provider, x.ProviderUserId }).IsUnique();


    }
}