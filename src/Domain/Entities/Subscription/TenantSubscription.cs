using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;
using Bookazone.Domain.Entities.Tenant;

namespace Bookazone.Domain.Entities.Subscription;

public class TenantSubscription : UserBaseEntity
{
    public Guid FkTenantId { get; set; }
    public Tenants FkTenant { get; set; } = null!;
    public Guid FkPlanId { get; set; }
    public decimal Amount { get; set; }             // Price for the plan
    public SubscriptionPlan FkPlan { get; set; } = null!;
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; } 
    public DateTime? TrialEndsAt { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsTrial { get; set; } = false;
    public bool AutoRenew { get; set; } = true;    // Whether it auto-renews

    [MaxLength(100)] public string? Currency { get; set; }           // e.g., USD, SLL
    [MaxLength(200)] public string? ExternalSubscriptionId { get; set; } // e.g. Stripe subscription id
    [MaxLength(200)] public string? ExternalCustomerId { get; set; }
    
    [MaxLength(100)] public string PlanName { get; set; } = null!;   // e.g., Basic, Pro, Premium
    [MaxLength(2000)] public string? Notes { get; set; }             // Optional notes


    
    
}





