using Bookazone.Application.Interfaces.Repository;
using Bookazone.Domain.Entities.Vehicles;

namespace Bookazone.Application.Interfaces.Repository.Vehicles;

public interface IVehicleSpecRepository : IGenericRepository<VehicleSpec>
{
    Task<List<VehicleSpec>> GetByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default);
}
