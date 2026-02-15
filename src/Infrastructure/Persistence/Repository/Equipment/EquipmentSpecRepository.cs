using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Interfaces.Repository.Equipment;
using Bookazone.Domain.Entities.Equipment;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Persistence.Repository;

namespace Bookazone.Infrastructure.Persistence.Repository.Equipment;

public class EquipmentSpecRepository(
    BookazoneDbContext context,
    ILogger<EquipmentSpecRepository> logger)
    : BaseRepository<EquipmentSpec>(context, logger), IEquipmentSpecRepository
{
    public Task<List<EquipmentSpec>> GetByEquipmentAsync(Guid equipmentId, CancellationToken cancellationToken = default)
    {
        return Context.Set<EquipmentSpec>()
            .Where(r => r.FkEquipmentId == equipmentId && r.Active && !r.Deleted)
            .OrderBy(r => r.SortOrder)
            .ToListAsync(cancellationToken);
    }
}
