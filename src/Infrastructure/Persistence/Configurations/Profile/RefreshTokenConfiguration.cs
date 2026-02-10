using Bookazone.Domain.Entities.Profile;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Profile;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens", "profile");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Token).HasMaxLength(5000);
        builder.Property(d => d.ReplacedByToken).HasMaxLength(5000);
        builder.Property(d => d.Revoked).HasMaxLength(3000);
        builder.Property(d => d.RevokeReason).HasMaxLength(500);
        builder.Property(d => d.Revoked).HasDefaultValue(false);
        builder.Property(d => d.Active).HasDefaultValue(false);
        builder.Property(d => d.Deleted).HasDefaultValue(false);


        // Relationships
        builder.HasOne(d => d.FkUser)
            .WithMany() 
            .HasForeignKey("FkUserId") 
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(d => d.FkDevice)
            .WithMany() 
            .HasForeignKey("FkDeviceId") 
            .OnDelete(DeleteBehavior.SetNull);
        builder.Property(d => d.CreatedBy).HasMaxLength(50);
        builder.Property(d => d.UpdatedBy).HasMaxLength(50);
        builder.Property(d => d.DeletedBy).HasMaxLength(50);
        builder.Property(d => d.DeletedReason).HasMaxLength(500);
    }
}
