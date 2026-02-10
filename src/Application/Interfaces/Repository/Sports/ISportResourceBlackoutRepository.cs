using Bookazone.Application.Interfaces.Repository;
using Bookazone.Domain.Entities.Sports;

namespace Bookazone.Application.Interfaces.Repository.Sports;

public interface ISportResourceBlackoutRepository : IGenericRepository<SportResourceBlackout>
{
    Task<List<SportResourceBlackout>> GetByResourceAsync(
        Guid resourceId,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        CancellationToken cancellationToken = default);

    Task<bool> HasOverlapAsync(
        Guid resourceId,
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken = default);
}
