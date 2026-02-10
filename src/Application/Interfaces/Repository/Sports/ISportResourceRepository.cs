using Bookazone.Application.Interfaces.Repository;
using Bookazone.Domain.Entities.Sports;

namespace Bookazone.Application.Interfaces.Repository.Sports;

public interface ISportResourceRepository : IGenericRepository<SportResource>
{
    Task<List<SportResource>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<List<SportResource>> GetBySportTypeAsync(Guid tenantSportTypeId, CancellationToken cancellationToken = default);
}
