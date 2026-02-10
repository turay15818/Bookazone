using Bookazone.Application.Interfaces.Repository;
using Bookazone.Domain.Entities.Vehicles;

namespace Bookazone.Application.Interfaces.Repository.Vehicles;

public interface IVehiclePricingRuleRepository : IGenericRepository<VehiclePricingRule>
{
    Task<List<VehiclePricingRule>> GetByVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default);
}
