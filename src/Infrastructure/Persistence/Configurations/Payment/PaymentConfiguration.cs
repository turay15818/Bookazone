using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Payment;

public class PaymentConfiguration : IEntityTypeConfiguration<Domain.Entities.Tenant.Payment>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Tenant.Payment> builder)
    {
        builder.ToTable("Payments", "tenants");
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.FkTenantSubscription).WithMany().HasForeignKey(e => e.FkTenantSubscriptionId).OnDelete(DeleteBehavior.Cascade);
        builder.Property(e => e.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Active).HasDefaultValue(true);
        builder.Property(e => e.Deleted).HasDefaultValue(false);
    }
}