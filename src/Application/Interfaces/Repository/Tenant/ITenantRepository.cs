using Bookazone.Domain.Entities.Tenant;

namespace Bookazone.Application.Interfaces.Repository.Tenant;

public interface ITenantRepository : IGenericRepository<Tenants>
{
    Task<Tenants?> GetByNameAsync(string name);
    Task<Tenants> VerifyTenantAsync(Guid id, bool verified, string verifiedBy);
    Task<Bookazone.Application.DTOs.TenantDetailsVm?> GetDetailsAsync(Guid tenantId);
}
