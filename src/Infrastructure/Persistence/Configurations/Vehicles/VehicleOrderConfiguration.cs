using Bookazone.Domain.Entities.Vehicles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Vehicles;

public class VehicleOrderConfiguration : IEntityTypeConfiguration<VehicleOrder>
{
    public void Configure(EntityTypeBuilder<VehicleOrder> builder)
    {
        builder.ToTable("VehicleOrders", "vehicles");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Currency).HasMaxLength(3).IsRequired();
        builder.Property(e => e.PickupAddress).HasMaxLength(300);
        builder.Property(e => e.DropoffAddress).HasMaxLength(300);
        builder.Property(e => e.CargoDescription).HasMaxLength(2000);
        builder.Property(e => e.ContactName).HasMaxLength(150);
        builder.Property(e => e.ContactEmail).HasMaxLength(200);
        builder.Property(e => e.ContactPhone).HasMaxLength(50);
        builder.Property(e => e.Notes).HasMaxLength(2000);

        builder.HasOne(e => e.FkVehicle)
            .WithMany()
            .HasForeignKey(e => e.FkVehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.FkTenant)
            .WithMany()
            .HasForeignKey(e => e.FkTenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.FkCustomer)
            .WithMany()
            .HasForeignKey(e => e.FkCustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.FkTenantId);
        builder.HasIndex(e => e.FkCustomerId);
        builder.HasIndex(e => e.FkVehicleId);
        builder.HasIndex(e => new { e.Status, e.ServiceType });
        builder.Property(e => e.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Active).HasDefaultValue(true);
        builder.Property(e => e.Deleted).HasDefaultValue(false);
    }
}
