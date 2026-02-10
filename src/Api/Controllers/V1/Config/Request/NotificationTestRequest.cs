using System.ComponentModel.DataAnnotations;

namespace Bookazone.Api.Controllers.V1.Config.Request;

public class NotificationTestRequest
{
    [Required, MaxLength(300), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [MaxLength(120)]
    public string? Title { get; set; }

    [MaxLength(500)]
    public string? Body { get; set; }
}
