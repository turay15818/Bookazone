
using Bookazone.Domain.Entities.Profile;

namespace Bookazone.Application.Interfaces.Repository.Profile;

public interface IAuthProviderRepository
{
    Task<AuthProvider?> GetByProviderAsync(string provider, string providerUserId, CancellationToken ct = default);
    Task<AuthProvider?> GetByUserAndProviderAsync(Guid userId, string provider, CancellationToken ct = default);
    Task AddAsync(AuthProvider entity, CancellationToken ct = default);
}
