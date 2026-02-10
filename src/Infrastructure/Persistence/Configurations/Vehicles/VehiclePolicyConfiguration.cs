using Bookazone.Domain.Entities.Vehicles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Vehicles;

public class VehiclePolicyConfiguration : IEntityTypeConfiguration<VehiclePolicy>
{
    public void Configure(EntityTypeBuilder<VehiclePolicy> builder)
    {
        builder.ToTable("VehiclePolicies", "vehicles");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Title).HasMaxLength(120).IsRequired();
        builder.Property(e => e.Body).HasMaxLength(2000);
        builder.HasOne(e => e.FkVehicle)
            .WithMany()
            .HasForeignKey(e => e.FkVehicleId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(e => new { e.FkVehicleId, e.SortOrder });
        builder.Property(e => e.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Active).HasDefaultValue(true);
        builder.Property(e => e.Deleted).HasDefaultValue(false);
    }
}
