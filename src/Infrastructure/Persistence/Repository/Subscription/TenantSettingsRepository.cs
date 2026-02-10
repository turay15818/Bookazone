using Microsoft.EntityFrameworkCore;
using Bookazone.Application.DTOs.Response;
using Bookazone.Application.Interfaces.Repository.Subscription;
using Bookazone.Domain.Entities.Tenant;
using Bookazone.Infrastructure.Persistence.DbContext;

namespace Bookazone.Infrastructure.Persistence.Repository.Subscription;

public class TenantSettingsRepository(BookazoneDbContext context, ILogger<TenantSettingsRepository> logger)
    : ITenantSettingsRepository
{
    public async Task<bool> AllowNegativeStockAsync(Guid tenantId)
    {
        return await context.TenantSettings
            .Where(t => t.Id == tenantId)
            .Select(t => t.AllowNegativeStock)
            .FirstOrDefaultAsync();
    }
    public async Task<VwTenantSettings?> GetByTenantAsync(Guid tenantId)
    {
        var settings = await context.TenantSettings
            .AsNoTracking()
            .Include(ts => ts.FkTenant)
            .FirstOrDefaultAsync(ts => ts.FkTenantId == tenantId);

        return settings == null
            ? null
            : new VwTenantSettings
            {
                TenantId = settings.FkTenantId,
                TenantName = settings.FkTenant?.Name,
                RestrictUsersByPlan = settings.RestrictUsersByPlan,
                DefaultTimeZone = settings.DefaultTimeZone,
                Currency = settings.Currency,
                DateUpdated = settings.DateUpdated
            };
    }

    public async Task<TenantSettings> CreateOrUpdateAsync(TenantSettings entity)
    {
        var existing = await context.TenantSettings
            .FirstOrDefaultAsync(ts => ts.FkTenantId == entity.FkTenantId);

        if (existing == null)
        {
            entity.Active = true;
            entity.Deleted = false;
            entity.DateCreated = DateTime.UtcNow;
            context.TenantSettings.Add(entity);
        }
        else
        {
            existing.RestrictUsersByPlan = entity.RestrictUsersByPlan;
            existing.DefaultTimeZone = entity.DefaultTimeZone;
            existing.Currency = entity.Currency;
            existing.AdditionalJson = entity.AdditionalJson;
            existing.DateUpdated = DateTime.UtcNow;
            context.TenantSettings.Update(existing);
        }

        await context.SaveChangesAsync();
        return entity;
    }
}
