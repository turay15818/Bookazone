using Bookazone.Domain.Enums;

namespace Bookazone.Application.DTOs.Response;

public class VwSportResourcePricingRule
{
    public Guid Id { get; set; }
    public Guid ResourceId { get; set; }
    public PricingRuleType RuleType { get; set; }
    public decimal PriceAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public DayOfWeek? DayOfWeek { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public int? MinDurationMinutes { get; set; }
}
