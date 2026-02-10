using Bookazone.Domain.Entities.Sports;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Sports;

public class SportResourceMediaConfiguration : IEntityTypeConfiguration<SportResourceMedia>
{
    public void Configure(EntityTypeBuilder<SportResourceMedia> builder)
    {
        builder.ToTable("SportResourceMedia", "sports");
        builder.HasKey(e => e.Id);
        builder.HasOne(e => e.FkSportResource)
            .WithMany()
            .HasForeignKey(e => e.FkSportResourceId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.FkStoredFile)
            .WithMany()
            .HasForeignKey(e => e.FkStoredFileId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(e => new { e.FkSportResourceId, e.SortOrder });
        builder.Property(e => e.DateCreated).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(e => e.Active).HasDefaultValue(true);
        builder.Property(e => e.Deleted).HasDefaultValue(false);
    }
}
