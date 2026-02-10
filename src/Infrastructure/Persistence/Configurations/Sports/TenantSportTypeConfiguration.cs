using Bookazone.Domain.Entities.Sports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Sports;

public class TenantSportTypeConfiguration : IEntityTypeConfiguration<TenantSportType>
{
    public void Configure(EntityTypeBuilder<TenantSportType> builder)
    {
        builder.ToTable("TenantSportTypes", "sports");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).HasMaxLength(150).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.HasOne(e => e.FkTenant)
            .WithMany()
            .HasForeignKey(e => e.FkTenantId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(e => e.FkTenant).AutoInclude();
        builder.HasOne(e => e.FkTenantBookingCategory)
            .WithMany()
            .HasForeignKey(e => e.FkTenantBookingCategoryId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(e => e.FkTenantId);
        builder.HasIndex(e => e.FkTenantBookingCategoryId);
        builder.HasIndex(e => new { e.FkTenantId, e.Name }).IsUnique();
        builder.Property(e => e.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Active).HasDefaultValue(true);
        builder.Property(e => e.Deleted).HasDefaultValue(false);
    }
}