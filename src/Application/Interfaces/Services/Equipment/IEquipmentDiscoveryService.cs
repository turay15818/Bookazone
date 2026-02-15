using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Equipment;
using Bookazone.Domain.Enums;

namespace Bookazone.Application.Interfaces.Services.Equipment;

public interface IEquipmentDiscoveryService
{
    Task<ApiResult> GetEquipmentsAsync(EquipmentSearchRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> GetEquipmentAsync(Guid equipmentId, CancellationToken cancellationToken = default);
    Task<ApiResult> GetEquipmentAvailabilityAsync(
        Guid equipmentId,
        DateTime startLocal,
        DateTime endLocal,
        EquipmentPricingUnit pricingUnit,
        int unitsRequested,
        CancellationToken cancellationToken = default);
}
