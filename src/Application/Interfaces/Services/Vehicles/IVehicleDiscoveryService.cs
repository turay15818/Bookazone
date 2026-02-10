using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Vehicles;

namespace Bookazone.Application.Interfaces.Services.Vehicles;

public interface IVehicleDiscoveryService
{
    Task<ApiResult> GetVehiclesAsync(VehicleSearchRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> GetVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default);
}
