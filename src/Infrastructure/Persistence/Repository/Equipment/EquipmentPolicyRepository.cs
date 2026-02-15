using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Interfaces.Repository.Equipment;
using Bookazone.Domain.Entities.Equipment;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Persistence.Repository;

namespace Bookazone.Infrastructure.Persistence.Repository.Equipment;

public class EquipmentPolicyRepository(
    BookazoneDbContext context,
    ILogger<EquipmentPolicyRepository> logger)
    : BaseRepository<EquipmentPolicy>(context, logger), IEquipmentPolicyRepository
{
    public Task<List<EquipmentPolicy>> GetByEquipmentAsync(Guid equipmentId, CancellationToken cancellationToken = default)
    {
        return Context.Set<EquipmentPolicy>()
            .Where(r => r.FkEquipmentId == equipmentId && r.Active && !r.Deleted)
            .OrderBy(r => r.SortOrder)
            .ToListAsync(cancellationToken);
    }
}
