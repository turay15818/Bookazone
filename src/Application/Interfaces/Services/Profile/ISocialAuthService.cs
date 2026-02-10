using Bookazone.Api.Controllers.V1.Config.Request;

namespace Bookazone.Application.Interfaces.Services.Profile;


public record SocialGoogleLoginRequest(string IdToken, Guid? TenantId, string? NotificationToken,  DeviceRequest DeviceRequest );

public interface ISocialAuthService
{
    Task<object> GoogleLoginAsync(SocialGoogleLoginRequest req, CancellationToken ct = default);
}