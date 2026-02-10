using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookazone.Api.Controllers.V1.Config;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Events;
using Bookazone.Application.Interfaces.Services.Events;
using Bookazone.Infrastructure.Authorization;

namespace Bookazone.Api.Controllers.V1.Secure.Events;

[Authorize]
[ApiController]
[ApiExplorerSettings(GroupName = SwaggerConfig.SwaggerDocName.Events.Slug)]
[Route(RouteWebVersion1.Secure.Events.Categories.Base)]
public class EventCategoryConfigController(
    IEventCategoryService eventCategoryService,
    ILogger<EventCategoryConfigController> logger)
    : ControllerBase
{
    [HttpGet]
    [PermissionAuthorize(Consts.Permissions.TenantSettingsManage)]
    [Route(RouteWebVersion1.Secure.Events.Categories.All)]
    public async Task<ApiResult> All()
    {
        try
        {
            return await eventCategoryService.GetEventCategoriesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting event categories");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpGet]
    [PermissionAuthorize(Consts.Permissions.TenantSettingsManage)]
    [Route(RouteWebVersion1.Secure.Events.Categories.Find)]
    public async Task<ApiResult> Find(Guid id)
    {
        try
        {
            return await eventCategoryService.GetEventCategoryAsync(id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error finding event category {CategoryId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.TenantSettingsManage)]
    [Route(RouteWebVersion1.Secure.Events.Categories.Create)]
    public async Task<ApiResult> Create([FromBody] EventCategoryCreateRequest request)
    {
        try
        {
            return await eventCategoryService.CreateEventCategoryAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating event category");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPut]
    [PermissionAuthorize(Consts.Permissions.TenantSettingsManage)]
    [Route(RouteWebVersion1.Secure.Events.Categories.Update)]
    public async Task<ApiResult> Update([FromBody] EventCategoryUpdateRequest request)
    {
        try
        {
            return await eventCategoryService.UpdateEventCategoryAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating event category {CategoryId}", request.Id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpDelete]
    [PermissionAuthorize(Consts.Permissions.TenantSettingsManage)]
    [Route(RouteWebVersion1.Secure.Events.Categories.Delete)]
    public async Task<ApiResult> Delete(Guid id)
    {
        try
        {
            return await eventCategoryService.DeleteEventCategoryAsync(id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting event category {CategoryId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
}
