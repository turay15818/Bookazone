using Bookazone.Application.Interfaces.Repository.Tenant;
using Bookazone.Domain.Entities.Tenant;
using Bookazone.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Bookazone.Infrastructure.Persistence.Repository.Tenant;


public class UserTenantRepository(BookazoneDbContext context) : IUserTenantRepository
{
    public async Task<List<UserTenant>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.UserTenants
            .AsNoTracking()
            .Where(ut =>
                ut.FkUserId == userId &&
                ut.Active &&
                !ut.Deleted)
            .Include(ut => ut.FkTenant)
            .ToListAsync(cancellationToken);
    }


    public async Task<UserTenant?> GetByUserAndTenantAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await context.UserTenants
            .AsNoTracking()
            .FirstOrDefaultAsync(ut =>
                ut.FkUserId == userId &&
                ut.TenantId == tenantId &&
                ut.Active &&
                !ut.Deleted,
                cancellationToken);
    }


    public async Task<bool> IsUserInTenantAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await context.UserTenants
            .AsNoTracking()
            .AnyAsync(ut =>
                ut.FkUserId == userId &&
                ut.TenantId == tenantId &&
                ut.Active &&
                !ut.Deleted,
                cancellationToken);
    }
}
