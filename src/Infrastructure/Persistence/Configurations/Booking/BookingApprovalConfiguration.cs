using Bookazone.Domain.Entities.Booking;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Booking;

public class BookingApprovalConfiguration : IEntityTypeConfiguration<BookingApproval>
{
    public void Configure(EntityTypeBuilder<BookingApproval> builder)
    {
        builder.ToTable("BookingApprovals", "booking");
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.FkBooking)
            .WithOne()
            .HasForeignKey<BookingApproval>(e => e.FkBookingId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(e => e.FkBookingId).IsUnique();
        builder.Property(e => e.Reason).HasMaxLength(1000);
        builder.Property(e => e.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Active).HasDefaultValue(true);
        builder.Property(e => e.Deleted).HasDefaultValue(false);
    }
}
