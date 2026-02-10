using Bookazone.Domain.Entities.Tenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Tenant;

public class TenantSettingsConfiguration : IEntityTypeConfiguration<TenantSettings>
{
    public void Configure(EntityTypeBuilder<TenantSettings> builder)
    {
        builder.ToTable("TenantSettings", "tenants");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Key).HasMaxLength(500).IsRequired();
        builder.Property(e => e.Value).HasMaxLength(2000).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.HasOne(e => e.FkTenant).WithOne(t => t.TenantSettings).HasForeignKey<TenantSettings>(e => e.FkTenantId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(e => e.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Active).HasDefaultValue(true);
        builder.Property(e => e.Deleted).HasDefaultValue(false);
    }
}
