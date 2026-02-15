using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookazone.Api.Controllers.V1.Config;
using Bookazone.Api.Controllers.V1.Config.Request;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.Interfaces.Services.Tenant;

namespace Bookazone.Api.Controllers.V1.Public;

[AllowAnonymous]
[ApiController]
[ApiExplorerSettings(GroupName = SwaggerConfig.SwaggerDocName.WebVersion1.Slug)]
[Route(RouteWebVersion1.Public.Tenants.Base)]
public class TenantsController(
    ITenantDiscoveryService tenantDiscoveryService,
    ILogger<TenantsController> logger)
    : ControllerBase
{
    [HttpGet]
    [Route(RouteWebVersion1.Public.Tenants.Full)]
    public async Task<ApiResult> Full(
        [FromRoute] Guid id
       )
    {
        try
        {
            return await tenantDiscoveryService.GetTenantFullDetailsAsync(
                id,
                HttpContext.RequestAborted);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting public tenant details {TenantId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
}