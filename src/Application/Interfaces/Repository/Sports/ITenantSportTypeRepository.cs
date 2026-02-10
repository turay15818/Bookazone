using Bookazone.Application.Interfaces.Repository;
using Bookazone.Domain.Entities.Sports;

namespace Bookazone.Application.Interfaces.Repository.Sports;

public interface ITenantSportTypeRepository : IGenericRepository<TenantSportType>
{
    Task<List<TenantSportType>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<TenantSportType?> GetByTenantAndNameAsync(Guid tenantId, string name, CancellationToken cancellationToken = default);
}
