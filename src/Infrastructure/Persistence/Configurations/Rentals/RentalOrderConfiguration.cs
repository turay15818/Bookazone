using Bookazone.Domain.Entities.Rentals;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Rentals;

public class RentalOrderConfiguration : IEntityTypeConfiguration<RentalOrder>
{
    public void Configure(EntityTypeBuilder<RentalOrder> builder)
    {
        builder.ToTable("RentalOrders", "rentals");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Currency).HasMaxLength(3).IsRequired();
        builder.Property(e => e.ContactName).HasMaxLength(150);
        builder.Property(e => e.ContactEmail).HasMaxLength(200);
        builder.Property(e => e.ContactPhone).HasMaxLength(50);
        builder.Property(e => e.Notes).HasMaxLength(2000);

        builder.HasOne(e => e.FkRental)
            .WithMany()
            .HasForeignKey(e => e.FkRentalId)
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
        builder.HasIndex(e => e.FkRentalId);
        builder.HasIndex(e => e.Status);
        builder.Property(e => e.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Active).HasDefaultValue(true);
        builder.Property(e => e.Deleted).HasDefaultValue(false);
    }
}
