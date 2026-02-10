using Bookazone.Application.Interfaces.Repository;
using Bookazone.Domain.Entities.Sports;

namespace Bookazone.Application.Interfaces.Repository.Sports;

public interface ISportResourceAvailabilityRepository : IGenericRepository<SportResourceAvailability>
{
    Task<List<SportResourceAvailability>> GetByResourceAsync(Guid resourceId, CancellationToken cancellationToken = default);
    Task<SportResourceAvailability?> GetByResourceAndDayAsync(Guid resourceId, DayOfWeek dayOfWeek, CancellationToken cancellationToken = default);
    Task<SportResourceAvailability> CreateOrUpdateAsync(SportResourceAvailability availability, CancellationToken cancellationToken = default);
}
