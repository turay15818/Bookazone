using Bookazone.Domain.Entities.Sports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Sports;

public class SportResourceConfiguration : IEntityTypeConfiguration<SportResource>
{
    public void Configure(EntityTypeBuilder<SportResource> builder)
    {
        builder.ToTable("SportResources", "sports");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).HasMaxLength(150).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.Property(e => e.Address).HasMaxLength(300);
        builder.HasOne(e => e.FkTenant)
            .WithMany()
            .HasForeignKey(e => e.FkTenantId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.FkTenantSportType)
            .WithMany()
            .HasForeignKey(e => e.FkTenantSportTypeId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(e => e.FkTenantId);
        builder.HasIndex(e => new { e.FkTenantId, e.FkTenantSportTypeId });
        builder.Property(e => e.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Active).HasDefaultValue(true);
        builder.Property(e => e.Deleted).HasDefaultValue(false);
    }
}
