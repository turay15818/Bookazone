using Bookazone.Domain.Common;
using Bookazone.Domain.Entities.Others;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bookazone.Infrastructure.Persistence.Configurations.Others;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        // Table configuration
        builder.ToTable("AuditLog", "audit");
        
        // Primary key
        builder.HasKey(a => a.Id);
        
        // Indexes for common queries
        builder.HasIndex(a => a.DateCreated)
            .HasDatabaseName("IX_AuditLog_DateCreated");
        
        builder.HasIndex(a => a.CreatedBy)
            .HasDatabaseName("IX_AuditLog_CreatedBy");
        
        builder.HasIndex(a => a.IpAddress)
            .HasDatabaseName("IX_AuditLog_IpAddress");
        
        builder.HasIndex(a => a.RequestMethod)
            .HasDatabaseName("IX_AuditLog_RequestMethod");
        
        builder.HasIndex(a => a.ResponseStatusCode)
            .HasDatabaseName("IX_AuditLog_ResponseStatusCode");
        
        // Composite index for common query patterns
        builder.HasIndex(a => new { a.CreatedBy, a.DateCreated })
            .HasDatabaseName("IX_AuditLog_CreatedBy_DateCreated");
        
        // Property configurations
        builder.Property(a => a.Name)
            .HasMaxLength(150)
            .IsRequired(false);
        
        builder.Property(a => a.IpAddress)
            .HasMaxLength(150)
            .IsRequired(false);
        
        builder.Property(a => a.Url)
            .HasMaxLength(10000)
            .IsRequired(false);
        
        builder.Property(a => a.QueryString)
            .HasMaxLength(1000)
            .IsRequired(false);
        
        builder.Property(a => a.Payload)
            .HasColumnType("text")
            .IsRequired(false);
        
        builder.Property(a => a.Response)
            .HasColumnType("text")
            .IsRequired(false);
        
        builder.Property(a => a.RequestHeaders)
            .HasColumnType("text")
            .IsRequired(false);
        
        builder.Property(a => a.RequestContentType)
            .HasMaxLength(200)
            .IsRequired(false);
        
        builder.Property(a => a.RequestMethod)
            .HasMaxLength(10)
            .IsRequired(false);
        
        builder.Property(a => a.ResponseStatusCode)
            .HasMaxLength(10)
            .IsRequired(false);
        
        builder.Property(a => a.ResponseHeaders)
            .HasColumnType("text")
            .IsRequired(false);
        
        builder.Property(a => a.ResponseContentType)
            .HasMaxLength(200)
            .IsRequired(false);
        
        builder.Property(a => a.ResponseTimestamp)
            .IsRequired(false);
        
        builder.Property(a => a.User)
            .HasMaxLength(1000)
            .IsRequired(false);
        
        // Base entity properties
        builder.Property(a => a.CreatedBy)
            .HasMaxLength(50)
            .IsRequired(false);
        
        builder.Property(a => a.UpdatedBy)
            .HasMaxLength(50)
            .IsRequired(false);
        
        builder.Property(a => a.DateCreated)
            .IsRequired();
        
        builder.Property(a => a.DateUpdated)
            .IsRequired(false);
        
        builder.Property(a => a.Active)
            .IsRequired()
            .HasDefaultValue(true);
        
        builder.Property(a => a.Deleted)
            .IsRequired()
            .HasDefaultValue(false);
        
        builder.Property(a => a.DateDeleted)
            .IsRequired(false);
        
        builder.Property(a => a.DeletedBy)
            .HasMaxLength(50)
            .IsRequired(false);
        
        builder.Property(a => a.DeletedReason)
            .HasMaxLength(500)
            .IsRequired(false);
    }
}