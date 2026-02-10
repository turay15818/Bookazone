using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Enums;

namespace Bookazone.Api.Controllers.V1.Config.Request;

public class SportResourcePricingRuleCreateRequest
{
    [Required] public Guid ResourceId { get; set; }
    [Required] public PricingRuleType RuleType { get; set; }
    [Required] public decimal PriceAmount { get; set; }
    [MaxLength(3)] public string? Currency { get; set; }
    public DayOfWeek? DayOfWeek { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public int? MinDurationMinutes { get; set; }
}

public class SportResourcePricingRuleUpdateRequest : SportResourcePricingRuleCreateRequest
{
    [Required] public Guid Id { get; set; }
}

public class SportResourcePricingRuleUpsertRequest
{
    public Guid? Id { get; set; }
    [Required] public Guid ResourceId { get; set; }
    [Required] public PricingRuleType RuleType { get; set; }
    [Required] public decimal PriceAmount { get; set; }
    [MaxLength(3)] public string? Currency { get; set; }
    public DayOfWeek? DayOfWeek { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public int? MinDurationMinutes { get; set; }
}
