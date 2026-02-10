using Bookazone.Domain.Entities.Tenant;

namespace Bookazone.Application.Interfaces.Repository.Tenant;

public interface IUserTenantRepository
{
    Task<List<UserTenant>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserTenant?> GetByUserAndTenantAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<bool> IsUserInTenantAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);
}
