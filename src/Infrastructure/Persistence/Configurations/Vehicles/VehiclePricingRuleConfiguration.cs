using Bookazone.Domain.Entities.Vehicles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Vehicles;

public class VehiclePricingRuleConfiguration : IEntityTypeConfiguration<VehiclePricingRule>
{
    public void Configure(EntityTypeBuilder<VehiclePricingRule> builder)
    {
        builder.ToTable("VehiclePricingRules", "vehicles");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Currency).HasMaxLength(3);
        builder.HasOne(e => e.FkVehicle)
            .WithMany()
            .HasForeignKey(e => e.FkVehicleId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(e => e.FkVehicleId);
        builder.Property(e => e.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Active).HasDefaultValue(true);
        builder.Property(e => e.Deleted).HasDefaultValue(false);
    }
}
