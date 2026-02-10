using Bookazone.Application.Interfaces.Repository.Profile;

namespace Bookazone.Infrastructure.Services;

public class TokenRevocationService(
    IRefreshTokenRepository refreshTokenRepository,
    ILogger<TokenRevocationService> logger)
{
    public async Task RevokeAllUserTokensAsync(Guid userId, string reason = "User initiated")
    {
        await refreshTokenRepository.RevokeAllForUserAsync(userId);
        logger.LogInformation("🔒 Revoked all refresh tokens for user {UserId}. Reason: {Reason}", userId, reason);
    }

    public async Task RevokeDeviceTokensAsync(Guid deviceId, string reason = "Device removed")
    {
        await refreshTokenRepository.RevokeAllForDeviceAsync(deviceId);
        logger.LogInformation("🔒 Revoked all refresh tokens for device {DeviceId}. Reason: {Reason}", deviceId, reason);
    }

    public async Task RevokeTenantTokensAsync(Guid tenantId, string reason = "Tenant policy change")
    {
        await refreshTokenRepository.RevokeAllForTenantAsync(tenantId);
        logger.LogInformation("🔒 Revoked all refresh tokens for tenant {TenantId}. Reason: {Reason}", tenantId, reason);
    }
}
