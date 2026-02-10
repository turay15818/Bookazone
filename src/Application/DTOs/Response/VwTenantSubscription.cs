namespace Bookazone.Application.DTOs.Response;

public class VwTenantSubscription
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string? TenantName { get; set; }

    public Guid PlanId { get; set; }
    public string PlanName { get; set; } = null!;
    public decimal PlanPrice { get; set; }
    public string PlanPeriod { get; set; } = "monthly";

    public bool IsActive { get; set; }
    public bool IsTrial { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? TrialEndsAt { get; set; }

    public string? ExternalSubscriptionId { get; set; }
}
