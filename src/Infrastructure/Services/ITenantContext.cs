using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.Interfaces.Repository.Tenant;
using Bookazone.Domain.Entities.Tenant;


namespace Bookazone.Infrastructure.Services;

public interface ITenantContext
{
    Tenants CurrentTenant { get; }
}

public class TenantContext : ITenantContext
{
    public Tenants CurrentTenant { get; private set; }

    public async Task InitializeAsync(HttpRequest request, ITenantRepository tenantRepository)
    {
        var host = request.Host.Host;
        var subdomain = host.Split('.')[0]; 
        var tenant = await tenantRepository.GetByNameAsync(subdomain);
        if (tenant == null)
            throw new Except(ErrorHttp.NotFound);
        
        CurrentTenant = tenant;
    }
}
