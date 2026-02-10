using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs;
using Bookazone.Application.Interfaces.Repository.Tenant;
using Bookazone.Domain.Entities.Tenant;
using Bookazone.Infrastructure.Persistence.DbContext;


namespace Bookazone.Infrastructure.Persistence.Repository.Tenant;

public class TenantRepository(
    BookazoneDbContext context,
    ILogger<TenantRepository> logger)
    : BaseRepository<Tenants>(context, logger), ITenantRepository
{
    public async Task<Tenants?> GetByNameAsync(string name)
    {
        var lower = name.ToLower();
        return await Context.Tenants.FirstOrDefaultAsync(t =>
            (t.Name.ToLower() == lower || t.Subdomain == lower)
            && !t.Deleted && t.Active);
    }

    public async Task<Tenants> VerifyTenantAsync(Guid id, bool verified, string verifiedBy)
    {
        var tenant = await Context.Tenants.FindAsync(id)
                     ?? throw new Except(ErrorHttp.NotFound);

        tenant.IsVerified = verified;
        tenant.UpdatedBy = verifiedBy;
        tenant.DateUpdated = DateTime.UtcNow;

        await Context.SaveChangesAsync();
        return tenant;
    }

    public async Task<TenantDetailsVm?> GetDetailsAsync(Guid tenantId)
    {
        var tenant = await Context.Tenants
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == tenantId && !t.Deleted);
        if (tenant == null) return null;

        /*var shops = await Context.Shops
            .Where(s => s.FkTenantId == tenantId && !s.Deleted)
            .Select(s => new TenantShopBriefVm
            {
                Id = s.Id,
                Name = s.Name,
                IsMainShop = s.IsMainShop,
                Active = s.Active,
                DateCreated = s.DateCreated
            }).ToListAsync();*/

        var users = await Context.Users
            .Where(u => u.FkTenantId == tenantId && !u.Deleted)
            .Select(u => new TenantUserBriefVm
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Phone = u.Phone,
                Active = u.Active
            }).ToListAsync();

        return new TenantDetailsVm
        {
            Tenant = new TenantBriefVm
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Email = tenant.Email,
                Phone = tenant.Phone,
                Address = tenant.Address,
                City = tenant.City,
                Country = tenant.Country,
                Logo = tenant.Logo,
                Subdomain = tenant.Subdomain,
                IsVerified = tenant.IsVerified,
                DateCreated = tenant.DateCreated
            },
          //  Shops = shops,
            Users = users,
          //  ShopCount = shops.Count,
            UserCount = users.Count
        };
    }
}
