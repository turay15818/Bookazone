using Bookazone.Application.Interfaces.Repository;
using Bookazone.Domain.Entities.Vehicles;

namespace Bookazone.Application.Interfaces.Repository.Vehicles;

public interface IVehicleMediaRepository : IGenericRepository<VehicleMedia>
{
    Task<List<VehicleMedia>> GetByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default);
}
