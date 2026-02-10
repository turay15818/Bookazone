using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookazone.Api.Controllers.V1.Config;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Events;
using Bookazone.Application.Interfaces.Services.Events;
using Bookazone.Domain.Entities.Events;
using Bookazone.Infrastructure.Authorization;

namespace Bookazone.Api.Controllers.V1.Secure.Events;

[Authorize]
[ApiController]
[ApiExplorerSettings(GroupName = SwaggerConfig.SwaggerDocName.Events.Slug)]
[Route(RouteWebVersion1.Secure.Events.Base)]
public class EventManagementController(
    IEventManagementService eventManagementService,
    ILogger<EventManagementController> logger)
    : ControllerBase
{
    [HttpGet]
    [PermissionAuthorize(Consts.Permissions.ResourceView)]
    [Route(RouteWebVersion1.Secure.Events.All)]
    public async Task<ApiResult> All(
        [FromQuery] List<EventStatus>? statuses = null,
        [FromQuery] DateTime? fromUtc = null,
        [FromQuery] DateTime? toUtc = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] string? search = null,
        [FromQuery] int? pageIndex = null,
        [FromQuery] int? pageSize = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = null)
    {
        try
        {
            return await eventManagementService.GetTenantEventsAsync(
                statuses,
                fromUtc,
                toUtc,
                categoryId,
                search,
                pageIndex,
                pageSize,
                sortBy,
                sortDirection);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting tenant events");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpGet]
    [PermissionAuthorize(Consts.Permissions.ResourceView)]
    [Route(RouteWebVersion1.Secure.Events.Find)]
    public async Task<ApiResult> Find(Guid id)
    {
        try
        {
            return await eventManagementService.GetEventAsync(id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting event {EventId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.ResourceCreate)]
    [Route(RouteWebVersion1.Secure.Events.Create)]
    public async Task<ApiResult> Create([FromBody] EventCreateRequest request)
    {
        try
        {
            return await eventManagementService.CreateEventAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating event for tenant {TenantId}", request.TenantId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPut]
    [PermissionAuthorize(Consts.Permissions.ResourceUpdate)]
    [Route(RouteWebVersion1.Secure.Events.Update)]
    public async Task<ApiResult> Update([FromBody] EventUpdateRequest request)
    {
        try
        {
            return await eventManagementService.UpdateEventAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating event {EventId}", request.Id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.ResourceUpdate)]
    [Route(RouteWebVersion1.Secure.Events.Publish)]
    public async Task<ApiResult> Publish([FromBody] EventPublishRequest request)
    {
        try
        {
            return await eventManagementService.PublishEventAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing event {EventId}", request.EventId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.ResourceUpdate)]
    [Route(RouteWebVersion1.Secure.Events.Cancel)]
    public async Task<ApiResult> Cancel([FromBody] EventCancelRequest request)
    {
        try
        {
            return await eventManagementService.CancelEventAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error cancelling event {EventId}", request.EventId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpDelete]
    [PermissionAuthorize(Consts.Permissions.ResourceDelete)]
    [Route(RouteWebVersion1.Secure.Events.Delete)]
    public async Task<ApiResult> Delete(Guid id)
    {
        try
        {
            return await eventManagementService.DeleteEventAsync(id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting event {EventId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.ResourceUpdate)]
    [Route(RouteWebVersion1.Secure.Events.Venue)]
    public async Task<ApiResult> UpsertVenue([FromRoute] Guid eventId, [FromBody] EventVenueUpsertRequest request)
    {
        try
        {
            return await eventManagementService.UpsertEventVenueAsync(eventId, request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error upserting venue for event {EventId}", eventId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.ResourceUpdate)]
    [Route(RouteWebVersion1.Secure.Events.TicketTypesBulk)]
    public async Task<ApiResult> UpsertTicketTypes(
        [FromRoute] Guid eventId,
        [FromBody] List<EventTicketTypeUpsertRequest> requests,
        [FromQuery] bool allOrNothing = false)
    {
        try
        {
            return await eventManagementService.UpsertEventTicketTypesAsync(eventId, requests, allOrNothing);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error upserting ticket types for event {EventId}", eventId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.ResourceUpdate)]
    [Route(RouteWebVersion1.Secure.Events.ScheduleBulk)]
    public async Task<ApiResult> UpsertSchedule(
        [FromRoute] Guid eventId,
        [FromBody] List<EventScheduleItemUpsertRequest> requests,
        [FromQuery] bool allOrNothing = false)
    {
        try
        {
            return await eventManagementService.UpsertEventScheduleAsync(eventId, requests, allOrNothing);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error upserting schedule for event {EventId}", eventId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.ResourceUpdate)]
    [Route(RouteWebVersion1.Secure.Events.PoliciesBulk)]
    public async Task<ApiResult> UpsertPolicies(
        [FromRoute] Guid eventId,
        [FromBody] List<EventPolicyUpsertRequest> requests,
        [FromQuery] bool allOrNothing = false)
    {
        try
        {
            return await eventManagementService.UpsertEventPoliciesAsync(eventId, requests, allOrNothing);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error upserting policies for event {EventId}", eventId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.ResourceUpdate)]
    [Route(RouteWebVersion1.Secure.Events.MediaBulk)]
    public async Task<ApiResult> UpsertMedia(
        [FromRoute] Guid eventId,
        [FromBody] List<EventMediaUpsertRequest> requests,
        [FromQuery] bool allOrNothing = false)
    {
        try
        {
            return await eventManagementService.UpsertEventMediaAsync(eventId, requests, allOrNothing);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error upserting media for event {EventId}", eventId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
}
