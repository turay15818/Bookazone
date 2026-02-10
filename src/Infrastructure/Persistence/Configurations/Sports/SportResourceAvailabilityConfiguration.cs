using Bookazone.Domain.Entities.Sports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Sports;

public class SportResourceAvailabilityConfiguration : IEntityTypeConfiguration<SportResourceAvailability>
{
    public void Configure(EntityTypeBuilder<SportResourceAvailability> builder)
    {
        builder.ToTable("SportResourceAvailabilities", "sports");
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.FkSportResource)
            .WithMany()
            .HasForeignKey(e => e.FkSportResourceId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(e => new { e.FkSportResourceId, e.DayOfWeek });
        builder.Property(e => e.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Active).HasDefaultValue(true);
        builder.Property(e => e.Deleted).HasDefaultValue(false);
    }
}
