using Bookazone.Domain.Entities.Booking;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Booking;

public class BookingConfiguration : IEntityTypeConfiguration<Domain.Entities.Booking.Booking>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Booking.Booking> builder)
    {
        builder.ToTable("Bookings", "booking", t => t.HasCheckConstraint("CK_Bookings_TimeRange", "\"StartUtc\" < \"EndUtc\""));
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Currency).HasMaxLength(3).IsRequired();
        builder.Property(e => e.Notes).HasMaxLength(200000);
        builder.Property(e => e.ExpectedAttendees);
        builder.HasOne(e => e.FkTenant)
            .WithMany()
            .HasForeignKey(e => e.FkTenantId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.FkSportResource)
            .WithMany()
            .HasForeignKey(e => e.FkSportResourceId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.FkCustomer)
            .WithMany()
            .HasForeignKey(e => e.FkCustomerId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(e => e.FkTenantId);
        builder.HasIndex(e => e.FkCustomerId);
        builder.HasIndex(e => new { e.FkTenantId, e.Status, e.StartUtc });
        builder.HasIndex(e => new { e.FkSportResourceId, e.StartUtc, e.EndUtc, e.Status });
        builder.Property(e => e.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Active).HasDefaultValue(true);
        builder.Property(e => e.Deleted).HasDefaultValue(false);
    }
}