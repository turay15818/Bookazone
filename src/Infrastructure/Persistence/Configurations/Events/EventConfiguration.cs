using Bookazone.Domain.Entities.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Events;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("Events", "events",
            t => t.HasCheckConstraint("CK_Events_TimeRange", "\"StartUtc\" < \"EndUtc\""));
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Title).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Subtitle).HasMaxLength(300);
        builder.Property(e => e.About).HasMaxLength(5000);
        builder.Property(e => e.TimeZoneId).HasMaxLength(100);
        builder.Property(e => e.City).HasMaxLength(100);
        builder.HasOne(e => e.FkTenant)
            .WithMany()
            .HasForeignKey(e => e.FkTenantId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.FkEventCategory)
            .WithMany()
            .HasForeignKey(e => e.FkEventCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.FkVenue)
            .WithMany()
            .HasForeignKey(e => e.FkVenueId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(e => e.FkCoverMedia)
            .WithMany()
            .HasForeignKey(e => e.FkCoverMediaId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(e => e.FkTenantId);
        builder.HasIndex(e => e.FkEventCategoryId);
        builder.HasIndex(e => new { e.Status, e.StartUtc });
        builder.HasIndex(e => new { e.FkTenantId, e.StartUtc });
        builder.Property(e => e.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Active).HasDefaultValue(true);
        builder.Property(e => e.Deleted).HasDefaultValue(false);
    }
}
