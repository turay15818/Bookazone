using Bookazone.Domain.Entities.Profile;

namespace Bookazone.Application.Interfaces.Repository.Profile;

public interface IRefreshTokenRepository
{
    Task<RefreshToken> CreateAsync(Guid userId, Guid deviceId);
    Task<RefreshToken?> GetAsync(string token);
    Task RevokeAsync(RefreshToken token);
    Task RevokeAllForUserAsync(Guid userId);
    Task RevokeAllForDeviceAsync(Guid deviceId);
    Task RevokeAllForTenantAsync(Guid tenantId);
    Task CleanUpOldTokensAsync(Guid userId, Guid deviceId);
}