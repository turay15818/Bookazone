using Bookazone.Domain.Entities.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Events;

public class EventOrderConfiguration : IEntityTypeConfiguration<EventOrder>
{
    public void Configure(EntityTypeBuilder<EventOrder> builder)
    {
        builder.ToTable("EventOrders", "events",
            t => t.HasCheckConstraint("CK_EventOrders_TotalQuantity", "\"TotalQuantity\" > 0"));
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Currency).HasMaxLength(3).IsRequired();
        builder.Property(e => e.Notes).HasMaxLength(2000);
        builder.Property(e => e.ContactName).HasMaxLength(150);
        builder.Property(e => e.ContactEmail).HasMaxLength(200);
        builder.Property(e => e.ContactPhone).HasMaxLength(50);
        builder.HasOne(e => e.FkEvent)
            .WithMany()
            .HasForeignKey(e => e.FkEventId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.FkTenant)
            .WithMany()
            .HasForeignKey(e => e.FkTenantId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.FkCustomer)
            .WithMany()
            .HasForeignKey(e => e.FkCustomerId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(e => e.FkEventId);
        builder.HasIndex(e => e.FkTenantId);
        builder.HasIndex(e => e.FkCustomerId);
        builder.HasIndex(e => e.ExpiresAtUtc);
        builder.HasIndex(e => new { e.FkTenantId, e.Status });
        builder.HasIndex(e => new { e.FkEventId, e.Status });
        builder.Property(e => e.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Active).HasDefaultValue(true);
        builder.Property(e => e.Deleted).HasDefaultValue(false);
    }
}
