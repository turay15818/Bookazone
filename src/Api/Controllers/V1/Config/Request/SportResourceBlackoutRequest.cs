using System.ComponentModel.DataAnnotations;

namespace Bookazone.Api.Controllers.V1.Config.Request;

public class SportResourceBlackoutCreateRequest
{
    [Required] public Guid ResourceId { get; set; }
    [Required] public DateTime StartUtc { get; set; }
    [Required] public DateTime EndUtc { get; set; }
    [MaxLength(250)] public string? Reason { get; set; }
}
