using Bookazone.Application.DTOs.Response;
using Bookazone.Domain.Entities.Subscription;

namespace Bookazone.Application.Interfaces.Repository.Subscription;

public interface ITenantSubscriptionRepository
{
    Task<VwTenantSubscription?> GetActiveForTenantAsync(Guid tenantId);
    Task<TenantSubscription> CreateAsync(TenantSubscription sub);
    Task<TenantSubscription> UpdateAsync(TenantSubscription sub);
    Task<IEnumerable<VwTenantSubscription>> ListByTenantAsync(Guid tenantId);
    Task<bool> DeactivateAsync(Guid id, string? deactivatedBy = null);
    Task CleanupExpiredAsync();
}