using Bookazone.Application.Interfaces.Repository;
using Bookazone.Domain.Entities.Sports;

namespace Bookazone.Application.Interfaces.Repository.Sports;

public interface ISportResourceMediaRepository : IGenericRepository<SportResourceMedia>
{
    Task<List<SportResourceMedia>> GetByResourceAsync(Guid resourceId, CancellationToken cancellationToken = default);
}
