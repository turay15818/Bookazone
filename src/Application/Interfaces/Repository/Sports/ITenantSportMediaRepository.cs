using Bookazone.Application.Interfaces.Repository;
using Bookazone.Domain.Entities.Sports;

namespace Bookazone.Application.Interfaces.Repository.Sports;

public interface ITenantSportMediaRepository : IGenericRepository<TenantSportMedia>
{
    Task<List<TenantSportMedia>> GetBySportTypeAsync(Guid tenantSportTypeId, CancellationToken cancellationToken = default);
}
