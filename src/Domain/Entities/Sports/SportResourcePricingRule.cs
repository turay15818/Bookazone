using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;
using Bookazone.Domain.Enums;

namespace Bookazone.Domain.Entities.Sports;

public class SportResourcePricingRule : UserBaseEntity
{
    public Guid FkSportResourceId { get; set; }
    public SportResource FkSportResource { get; set; } = null!;
    public PricingRuleType RuleType { get; set; } = PricingRuleType.Flat;
    public decimal PriceAmount { get; set; }
    [MaxLength(3)] public string Currency { get; set; } = "USD";
    public DayOfWeek? DayOfWeek { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public int? MinDurationMinutes { get; set; }
}
