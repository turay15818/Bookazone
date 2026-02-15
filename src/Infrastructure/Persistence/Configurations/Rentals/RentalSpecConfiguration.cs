using Bookazone.Domain.Entities.Rentals;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Rentals;

public class RentalSpecConfiguration : IEntityTypeConfiguration<RentalSpec>
{
    public void Configure(EntityTypeBuilder<RentalSpec> builder)
    {
        builder.ToTable("RentalSpecs", "rentals");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Label).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Value).HasMaxLength(300).IsRequired();

        builder.HasOne(e => e.FkRental)
            .WithMany()
            .HasForeignKey(e => e.FkRentalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.FkRentalId, e.SortOrder });
        builder.Property(e => e.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Active).HasDefaultValue(true);
        builder.Property(e => e.Deleted).HasDefaultValue(false);
    }
}
