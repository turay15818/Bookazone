using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Interfaces.Repository.Booking;
using Bookazone.Domain.Entities.Booking;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Persistence.Repository;

namespace Bookazone.Infrastructure.Persistence.Repository.Booking;

public class TenantBookingCategoryRepository(
    BookazoneDbContext context,
    ILogger<TenantBookingCategoryRepository> logger)
    : BaseRepository<TenantBookingCategory>(context, logger), ITenantBookingCategoryRepository
{
    public async Task<List<TenantBookingCategory>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await Context.TenantBookingCategories
            .AsNoTracking()
            .Include(tbc => tbc.FkBookingCategory)
            .Where(tbc =>
                tbc.FkTenantId == tenantId &&
                tbc.Active &&
                !tbc.Deleted)
            .OrderBy(tbc => tbc.FkBookingCategory.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TenantBookingCategory>> GetEnabledByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await Context.TenantBookingCategories
            .AsNoTracking()
            .Include(tbc => tbc.FkBookingCategory)
            .Where(tbc =>
                tbc.FkTenantId == tenantId &&
                tbc.IsEnabled &&
                tbc.Active &&
                !tbc.Deleted)
            .OrderBy(tbc => tbc.FkBookingCategory.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<TenantBookingCategory?> GetByTenantAndCategoryAsync(Guid tenantId, Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await Context.TenantBookingCategories
            .AsNoTracking()
            .Include(tbc => tbc.FkBookingCategory)
            .FirstOrDefaultAsync(tbc =>
                tbc.FkTenantId == tenantId &&
                tbc.FkBookingCategoryId == categoryId &&
                tbc.Active &&
                !tbc.Deleted,
                cancellationToken);
    }
}
