using Bookazone.Application.Interfaces.Repository;
using Bookazone.Domain.Entities.Vehicles;

namespace Bookazone.Application.Interfaces.Repository.Vehicles;

public interface IVehicleRepository : IGenericRepository<Vehicle>
{
    Task<List<Vehicle>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
