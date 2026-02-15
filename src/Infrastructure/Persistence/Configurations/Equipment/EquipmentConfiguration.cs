using Bookazone.Domain.Entities.Equipment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Equipment;

public class EquipmentConfiguration : IEntityTypeConfiguration<Bookazone.Domain.Entities.Equipment.Equipment>
{
    public void Configure(EntityTypeBuilder<Bookazone.Domain.Entities.Equipment.Equipment> builder)
    {
        builder.ToTable("Equipment", "equipment");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Title).HasMaxLength(150).IsRequired();
        builder.Property(e => e.Subtitle).HasMaxLength(300);
        builder.Property(e => e.Description).HasMaxLength(5000);
        builder.Property(e => e.Category).HasMaxLength(100);
        builder.Property(e => e.City).HasMaxLength(100);
        builder.Property(e => e.Address).HasMaxLength(300);
        builder.Property(e => e.Currency).HasMaxLength(3);

        builder.HasOne(e => e.FkTenant)
            .WithMany()
            .HasForeignKey(e => e.FkTenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.FkCoverMedia)
            .WithMany()
            .HasForeignKey(e => e.FkCoverMediaId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => e.FkTenantId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.Category);
        builder.HasIndex(e => e.City);
        builder.Property(e => e.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Active).HasDefaultValue(true);
        builder.Property(e => e.Deleted).HasDefaultValue(false);
    }
}
