using Bookazone.Domain.Entities.Rentals;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Rentals;

public class RentalPolicyConfiguration : IEntityTypeConfiguration<RentalPolicy>
{
    public void Configure(EntityTypeBuilder<RentalPolicy> builder)
    {
        builder.ToTable("RentalPolicies", "rentals");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Title).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Body).HasMaxLength(2000);

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
