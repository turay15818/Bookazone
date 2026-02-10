using Bookazone.Domain.Entities.Sports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Sports;

public class TenantSportMediaConfiguration : IEntityTypeConfiguration<TenantSportMedia>
{
    public void Configure(EntityTypeBuilder<TenantSportMedia> builder)
    {
        builder.ToTable("TenantSportMedia", "sports");
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.FkTenantSportType)
            .WithMany()
            .HasForeignKey(e => e.FkTenantSportTypeId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.FkStoredFile)
            .WithMany()
            .HasForeignKey(e => e.FkStoredFileId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(e => new { e.FkTenantSportTypeId, e.SortOrder });
        builder.Property(e => e.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Active).HasDefaultValue(true);
        builder.Property(e => e.Deleted).HasDefaultValue(false);
    }
}
