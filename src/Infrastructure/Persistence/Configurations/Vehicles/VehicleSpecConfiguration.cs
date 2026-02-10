using Bookazone.Domain.Entities.Vehicles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Vehicles;

public class VehicleSpecConfiguration : IEntityTypeConfiguration<VehicleSpec>
{
    public void Configure(EntityTypeBuilder<VehicleSpec> builder)
    {
        builder.ToTable("VehicleSpecs", "vehicles");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Label).HasMaxLength(120).IsRequired();
        builder.Property(e => e.Value).HasMaxLength(300).IsRequired();
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
