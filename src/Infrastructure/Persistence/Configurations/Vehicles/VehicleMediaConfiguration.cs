using Bookazone.Domain.Entities.Vehicles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Vehicles;

public class VehicleMediaConfiguration : IEntityTypeConfiguration<VehicleMedia>
{
    public void Configure(EntityTypeBuilder<VehicleMedia> builder)
    {
        builder.ToTable("VehicleMedia", "vehicles");
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.FkVehicle)
            .WithMany()
            .HasForeignKey(e => e.FkVehicleId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.FkStoredFile)
            .WithMany()
            .HasForeignKey(e => e.FkStoredFileId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(e => new { e.FkVehicleId, e.SortOrder });
        builder.Property(e => e.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Active).HasDefaultValue(true);
        builder.Property(e => e.Deleted).HasDefaultValue(false);
    }
}
