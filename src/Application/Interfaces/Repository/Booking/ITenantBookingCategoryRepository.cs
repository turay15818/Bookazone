using Bookazone.Application.Interfaces.Repository;
using Bookazone.Domain.Entities.Booking;

namespace Bookazone.Application.Interfaces.Repository.Booking;

public interface ITenantBookingCategoryRepository : IGenericRepository<TenantBookingCategory>
{
    Task<List<TenantBookingCategory>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<List<TenantBookingCategory>> GetEnabledByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<TenantBookingCategory?> GetByTenantAndCategoryAsync(Guid tenantId, Guid categoryId, CancellationToken cancellationToken = default);
}
