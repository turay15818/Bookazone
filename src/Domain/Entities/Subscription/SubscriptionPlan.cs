using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;

namespace Bookazone.Domain.Entities.Subscription;


public class SubscriptionPlan : UserBaseEntity
{
    [MaxLength(100)] public string Name { get; set; } = null!;
    [MaxLength(500)] public string? Description { get; set; }
    public decimal PricePerPeriod { get; set; } 
    [MaxLength(10)] public string Period { get; set; } = "monthly"; 
    public int? UserLimit { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsPublic { get; set; } = true; 
  [MaxLength(Int32.MaxValue)]  public string? MetadataJson { get; set; }
    public ICollection<TenantSubscription> TenantSubscriptions { get; set; } = new List<TenantSubscription>();
}
