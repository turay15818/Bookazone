using Bookazone.Domain.Entities.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Events;

public class EventOrderItemConfiguration : IEntityTypeConfiguration<EventOrderItem>
{
    public void Configure(EntityTypeBuilder<EventOrderItem> builder)
    {
        builder.ToTable("EventOrderItems", "events",
            t => t.HasCheckConstraint("CK_EventOrderItems_Quantity", "\"Quantity\" > 0"));
        builder.HasKey(e => e.Id);
        builder.Property(e => e.TicketName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Currency).HasMaxLength(3).IsRequired();
        builder.HasOne(e => e.FkEventOrder)
            .WithMany()
            .HasForeignKey(e => e.FkEventOrderId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.FkEventTicketType)
            .WithMany()
            .HasForeignKey(e => e.FkEventTicketTypeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(e => e.FkEventOrderId);
        builder.HasIndex(e => e.FkEventTicketTypeId);
        builder.Property(e => e.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Active).HasDefaultValue(true);
        builder.Property(e => e.Deleted).HasDefaultValue(false);
    }
}
