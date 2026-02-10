using Bookazone.Domain.Entities.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Events;

public class EventTicketTypeConfiguration : IEntityTypeConfiguration<EventTicketType>
{
    public void Configure(EntityTypeBuilder<EventTicketType> builder)
    {
        builder.ToTable("EventTicketTypes", "events");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Currency).HasMaxLength(3).IsRequired();
        builder.Property(e => e.Perks).HasMaxLength(2000);
        builder.HasOne(e => e.FkEvent)
            .WithMany()
            .HasForeignKey(e => e.FkEventId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(e => e.FkEventId);
        builder.HasIndex(e => new { e.FkEventId, e.Name }).IsUnique();
        builder.Property(e => e.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Active).HasDefaultValue(true);
        builder.Property(e => e.Deleted).HasDefaultValue(false);
    }
}
