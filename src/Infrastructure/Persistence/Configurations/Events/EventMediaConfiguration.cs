using Bookazone.Domain.Entities.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Events;

public class EventMediaConfiguration : IEntityTypeConfiguration<EventMedia>
{
    public void Configure(EntityTypeBuilder<EventMedia> builder)
    {
        builder.ToTable("EventMedia", "events");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Url).HasMaxLength(500).IsRequired();
        builder.Property(e => e.Caption).HasMaxLength(200);
        builder.HasOne(e => e.FkEvent)
            .WithMany()
            .HasForeignKey(e => e.FkEventId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(e => e.FkEventId);
        builder.HasIndex(e => new { e.FkEventId, e.SortOrder });
        builder.HasIndex(e => new { e.FkEventId, e.IsCover });
        builder.Property(e => e.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Active).HasDefaultValue(true);
        builder.Property(e => e.Deleted).HasDefaultValue(false);
    }
}
