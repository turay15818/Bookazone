using Bookazone.Application.DTOs.Response;
using Bookazone.Domain.Entities.Tenant;

namespace Bookazone.Application.Interfaces.Repository.Subscription;

public interface ITenantSettingsRepository
{
    Task<VwTenantSettings?> GetByTenantAsync(Guid tenantId);
    Task<TenantSettings> CreateOrUpdateAsync(TenantSettings settings);
    Task<bool> AllowNegativeStockAsync(Guid tenantId);

}