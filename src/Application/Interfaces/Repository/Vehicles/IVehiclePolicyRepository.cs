using Bookazone.Application.Interfaces.Repository;
using Bookazone.Domain.Entities.Vehicles;

namespace Bookazone.Application.Interfaces.Repository.Vehicles;

public interface IVehiclePolicyRepository : IGenericRepository<VehiclePolicy>
{
    Task<List<VehiclePolicy>> GetByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default);
}
