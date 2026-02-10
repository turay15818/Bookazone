using Bookazone.Domain.Entities.Tenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Tenant;

public class TenantConfiguration : IEntityTypeConfiguration<Tenants>
{
    public void Configure(EntityTypeBuilder<Tenants> builder)
    {
        builder.ToTable("Tenants", "tenants");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.Property(e => e.Email).HasMaxLength(100);
        builder.Property(e => e.Phone).HasMaxLength(20);
        builder.Property(e => e.Address).HasMaxLength(200);
        builder.Property(e => e.City).HasMaxLength(100);
        builder.Property(e => e.State).HasMaxLength(100);
        builder.Property(e => e.ZipCode).HasMaxLength(20);
        builder.Property(e => e.Country).HasMaxLength(100);
        builder.Property(e => e.Logo).HasMaxLength(5000);
        builder.Property(e => e.Subdomain).HasMaxLength(100);
        builder.Property(e => e.Code).HasMaxLength(4);
        builder.HasMany(e => e.TenantSubscriptions).WithOne(ts => ts.FkTenant).HasForeignKey(ts => ts.FkTenantId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.TenantSettings).WithOne(ts => ts.FkTenant).HasForeignKey<TenantSettings>(ts => ts.FkTenantId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(e => e.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Active).HasDefaultValue(true);
        builder.Property(e => e.Deleted).HasDefaultValue(false);
    }
}
