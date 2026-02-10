using Bookazone.Domain.Entities.Tenant;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Tenant;

public class TenantWorkingHourConfiguration : IEntityTypeConfiguration<TenantWorkingHour>
{
    public void Configure(EntityTypeBuilder<TenantWorkingHour> builder)
    {
        builder.ToTable("TenantWorkingHours", "tenants",
            t => t.HasCheckConstraint("CK_TenantWorkingHours_TimeRange",
                "\"IsClosed\" OR (\"OpenTime\" IS NOT NULL AND \"CloseTime\" IS NOT NULL AND \"OpenTime\" < \"CloseTime\")"));
        builder.HasKey(e => e.Id);
        builder.Property(e => e.SlotDurationMinutes).IsRequired();
        builder.HasOne(e => e.FkTenant)
            .WithMany()
            .HasForeignKey(e => e.FkTenantId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(e => e.FkTenantId);
        builder.HasIndex(e => new { e.FkTenantId, e.DayOfWeek }).IsUnique();
        builder.Property(e => e.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Active).HasDefaultValue(true);
        builder.Property(e => e.Deleted).HasDefaultValue(false);
    }
}
