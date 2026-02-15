using Bookazone.Api.Controllers.V1.Config.Request;
using Bookazone.Application.Common.Shared.Utils;

namespace Bookazone.Application.Interfaces.Services.Tenant;

public interface ITenantDiscoveryService
{
    Task<ApiResult> GetTenantFullDetailsAsync(Guid tenantId, CancellationToken cancellationToken = default);
}