using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookazone.Api.Controllers.V1.Config;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Events;
using Bookazone.Application.Interfaces.Services.Events;

namespace Bookazone.Api.Controllers.V1.Secure.Events;

[Authorize]
[ApiController]
[ApiExplorerSettings(GroupName = SwaggerConfig.SwaggerDocName.Events.Slug)]
[Route(RouteWebVersion1.Secure.Events.Subscriptions.Base)]
public class EventSubscriptionController(
    IEventSubscriptionService eventSubscriptionService,
    ILogger<EventSubscriptionController> logger)
    : ControllerBase
{
    [HttpGet]
    [Route(RouteWebVersion1.Secure.Events.Subscriptions.My)]
    public async Task<ApiResult> My()
    {
        try
        {
            return await eventSubscriptionService.GetMySubscriptionsAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting event subscriptions");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [Route(RouteWebVersion1.Secure.Events.Subscriptions.Upsert)]
    public async Task<ApiResult> Upsert([FromBody] EventCategorySubscriptionUpsertRequest request)
    {
        try
        {
            return await eventSubscriptionService.UpsertSubscriptionAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error upserting event subscription for category {CategoryId}", request.EventCategoryId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [Route(RouteWebVersion1.Secure.Events.Subscriptions.Bulk)]
    public async Task<ApiResult> Bulk(
        [FromBody] List<EventCategorySubscriptionUpsertRequest> requests,
        [FromQuery] bool allOrNothing = false)
    {
        try
        {
            return await eventSubscriptionService.UpsertSubscriptionsAsync(requests, allOrNothing);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error bulk upserting event subscriptions");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
}
