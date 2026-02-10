using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookazone.Api.Controllers.V1.Config;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Events;
using Bookazone.Application.Interfaces.Services.Events;

namespace Bookazone.Api.Controllers.V1.Public;

[AllowAnonymous]
[ApiController]
[ApiExplorerSettings(GroupName = SwaggerConfig.SwaggerDocName.Events.Slug)]
[Route(RouteWebVersion1.Public.Events.Base)]
public class EventsController(
    IEventDiscoveryService eventDiscoveryService,
    ILogger<EventsController> logger)
    : ControllerBase
{
    [HttpGet]
    [Route(RouteWebVersion1.Public.Events.Categories)]
    public async Task<ApiResult> Categories()
    {
        try
        {
            return await eventDiscoveryService.GetEventCategoriesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting event categories");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpGet]
    [Route(RouteWebVersion1.Public.Events.All)]
    public async Task<ApiResult> All(
        [FromQuery] Guid? categoryId = null,
        [FromQuery] string? categoryCode = null,
        [FromQuery] Guid? tenantId = null,
        [FromQuery] string? city = null,
        [FromQuery] string? search = null,
        [FromQuery] DateTime? fromUtc = null,
        [FromQuery] DateTime? toUtc = null,
        [FromQuery] int? pageIndex = null,
        [FromQuery] int? pageSize = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = null)
    {
        try
        {
            var request = new EventSearchRequest
            {
                TenantId = tenantId,
                CategoryId = categoryId,
                CategoryCode = categoryCode,
                City = city,
                Search = search,
                FromUtc = fromUtc,
                ToUtc = toUtc,
                PageIndex = pageIndex,
                PageSize = pageSize,
                SortBy = sortBy,
                SortDirection = sortDirection
            };

            return await eventDiscoveryService.GetEventsAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting events");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpGet]
    [Route(RouteWebVersion1.Public.Events.Find)]
    public async Task<ApiResult> Find([FromRoute] Guid id)
    {
        try
        {
            return await eventDiscoveryService.GetEventAsync(id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting event {EventId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpGet]
    [Route(RouteWebVersion1.Public.Events.Similar)]
    public async Task<ApiResult> Similar([FromRoute] Guid id, [FromQuery] int? take = null)
    {
        try
        {
            return await eventDiscoveryService.GetSimilarEventsAsync(id, take);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting similar events for {EventId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
}
