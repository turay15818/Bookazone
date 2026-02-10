using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Entities.Profile;

namespace Bookazone.Domain.Common;

public class BaseEntity
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    [MaxLength(50)] public string? CreatedBy { get; set; }
    [MaxLength(50)] public string? UpdatedBy { get; set; }
    public DateTime? DateCreated { get; set; } = null;
    public DateTime? DateUpdated { get; set; } = null;
    public bool Active { get; set; } = true;
    public bool Deleted { get; set; } = false;
    public DateTime? DateDeleted { get; set; }
    public string? DeletedBy { get; set; }
    [MaxLength(500)] public string? DeletedReason { get; set; }
}