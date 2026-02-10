using System.ComponentModel.DataAnnotations;

namespace Bookazone.Api.Controllers.V1.Config.Request;


public class BiometricAuthRequest : BaseRequest
{
    [MaxLength(500)] public string? Username { get; set; }

    [MaxLength(2048)] public string? BiometricId { get; set; }

    [MaxLength(3000)] public string? NotificationToken { get; set; }
}