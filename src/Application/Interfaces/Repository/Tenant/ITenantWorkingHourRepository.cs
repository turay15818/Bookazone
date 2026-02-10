using Bookazone.Domain.Entities.Tenant;

namespace Bookazone.Application.Interfaces.Repository.Tenant;

public interface ITenantWorkingHourRepository : IGenericRepository<TenantWorkingHour>
{
    Task<List<TenantWorkingHour>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<TenantWorkingHour?> GetByTenantAndDayAsync(Guid tenantId, DayOfWeek dayOfWeek, CancellationToken cancellationToken = default);
    Task<TenantWorkingHour> CreateOrUpdateAsync(TenantWorkingHour workingHour, CancellationToken cancellationToken = default);
    Task<bool> IsValidBookingSlotAsync(Guid tenantId, DateTime startLocal, DateTime endLocal, CancellationToken cancellationToken = default);
}
