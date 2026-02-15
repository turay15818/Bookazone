using Bookazone.Domain.Entities.Rentals;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Rentals;

public class RentalMediaConfiguration : IEntityTypeConfiguration<RentalMedia>
{
    public void Configure(EntityTypeBuilder<RentalMedia> builder)
    {
        builder.ToTable("RentalMedia", "rentals");
        builder.HasKey(e => e.Id);

        builder.HasOne(e => e.FkRental)
            .WithMany()
            .HasForeignKey(e => e.FkRentalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.FkStoredFile)
            .WithMany()
            .HasForeignKey(e => e.FkStoredFileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.FkRentalId, e.SortOrder });
        builder.Property(e => e.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Active).HasDefaultValue(true);
        builder.Property(e => e.Deleted).HasDefaultValue(false);
    }
}
