namespace Bookazone.Application.DTOs.Response;

public class VwSubscriptionPlan
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal PricePerPeriod { get; set; }
    public string Period { get; set; } = "monthly";
    public int? UserLimit { get; set; }
    public bool IsPublic { get; set; }
    public bool IsActive { get; set; }
}
