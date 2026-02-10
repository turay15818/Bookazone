using Bookazone.Domain.Entities.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Events;

public class EventScheduleItemConfiguration : IEntityTypeConfiguration<EventScheduleItem>
{
    public void Configure(EntityTypeBuilder<EventScheduleItem> builder)
    {
        builder.ToTable("EventScheduleItems", "events");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Title).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Subtitle).HasMaxLength(300);
        builder.HasOne(e => e.FkEvent)
            .WithMany()
            .HasForeignKey(e => e.FkEventId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(e => e.FkEventId);
        builder.HasIndex(e => new { e.FkEventId, e.SortOrder });
        builder.HasIndex(e => new { e.FkEventId, e.StartUtc });
        builder.Property(e => e.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Active).HasDefaultValue(true);
        builder.Property(e => e.Deleted).HasDefaultValue(false);
    }
}
