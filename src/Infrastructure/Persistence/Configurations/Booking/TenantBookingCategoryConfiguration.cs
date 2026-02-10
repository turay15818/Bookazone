using Bookazone.Domain.Entities.Booking;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Booking;

public class TenantBookingCategoryConfiguration : IEntityTypeConfiguration<TenantBookingCategory>
{
    public void Configure(EntityTypeBuilder<TenantBookingCategory> builder)
    {
        builder.ToTable("TenantBookingCategories", "booking");
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.FkTenant)
            .WithMany()
            .HasForeignKey(e => e.FkTenantId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(e => e.FkTenant)
            .AutoInclude();
        builder.HasOne(e => e.FkBookingCategory)
            .WithMany()
            .HasForeignKey(e => e.FkBookingCategoryId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(e => e.FkTenantId);
        builder.HasIndex(e => new { e.FkTenantId, e.FkBookingCategoryId }).IsUnique();
        builder.Property(e => e.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Active).HasDefaultValue(true);
        builder.Property(e => e.Deleted).HasDefaultValue(false);
    }
}