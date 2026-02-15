using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Interfaces.Repository.Equipment;
using Bookazone.Domain.Entities.Equipment;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Persistence.Repository;

namespace Bookazone.Infrastructure.Persistence.Repository.Equipment;

public class EquipmentMediaRepository(
    BookazoneDbContext context,
    ILogger<EquipmentMediaRepository> logger)
    : BaseRepository<EquipmentMedia>(context, logger), IEquipmentMediaRepository
{
    public Task<List<EquipmentMedia>> GetByEquipmentAsync(Guid equipmentId, CancellationToken cancellationToken = default)
    {
        return Context.Set<EquipmentMedia>()
            .Include(m => m.FkStoredFile)
            .Where(m => m.FkEquipmentId == equipmentId && m.Active && !m.Deleted)
            .OrderBy(m => m.SortOrder)
            .ToListAsync(cancellationToken);
    }
}
