using Bookazone.Domain.Entities.Subscription;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Tenant;

public class TenantSubscriptionConfiguration : IEntityTypeConfiguration<TenantSubscription>
{
    public void Configure(EntityTypeBuilder<TenantSubscription> builder)
    {
        builder.ToTable("TenantSubscriptions", "tenants");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.PlanName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Currency).HasMaxLength(50);
        builder.Property(e => e.Notes).HasMaxLength(2000);
        builder.HasOne(e => e.FkTenant).WithMany(t => t.TenantSubscriptions).HasForeignKey(e => e.FkTenantId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(e => e.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Active).HasDefaultValue(true);
        builder.Property(e => e.Deleted).HasDefaultValue(false);
    }
}
