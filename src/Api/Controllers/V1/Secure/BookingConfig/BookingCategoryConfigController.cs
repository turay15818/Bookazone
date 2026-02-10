using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookazone.Api.Controllers.V1.Config;
using Bookazone.Api.Controllers.V1.Config.Request;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.Interfaces.Services.Booking;
using Bookazone.Infrastructure.Authorization;

namespace Bookazone.Api.Controllers.V1.Secure.BookingConfig;

[Authorize]
[ApiController]
[ApiExplorerSettings(GroupName = SwaggerConfig.SwaggerDocName.WebVersion1.Slug)]
[Route(RouteWebVersion1.Secure.BookingConfig.Category.Base)]
public class BookingCategoryConfigController(
    IBookingConfigurationService bookingConfigurationService,
    ILogger<BookingCategoryConfigController> logger)
    : ControllerBase
{
    [HttpGet]
    [PermissionAuthorize(Consts.Permissions.ProfileView)]
    [Route(RouteWebVersion1.Secure.BookingConfig.Category.All)]
    public async Task<ApiResult> All()
    {
        try
        {
            return await bookingConfigurationService.GetBookingCategoriesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting booking categories");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpGet]
    [PermissionAuthorize(Consts.Permissions.ProfileView)]
    [Route(RouteWebVersion1.Secure.BookingConfig.Category.Find)]
    public async Task<ApiResult> Find(Guid id)
    {
        try
        {
            return await bookingConfigurationService.GetBookingCategoryAsync(id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error finding booking category {CategoryId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.TenantSettingsManage)]
    [Route(RouteWebVersion1.Secure.BookingConfig.Category.Create)]
    public async Task<ApiResult> Create([FromBody] BookingCategoryCreateRequest request)
    {
        try
        {
            return await bookingConfigurationService.CreateBookingCategoryAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating booking category");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPut]
    [PermissionAuthorize(Consts.Permissions.TenantSettingsManage)]
    [Route(RouteWebVersion1.Secure.BookingConfig.Category.Update)]
    public async Task<ApiResult> Update([FromBody] BookingCategoryUpdateRequest request)
    {
        try
        {
            return await bookingConfigurationService.UpdateBookingCategoryAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating booking category {CategoryId}", request.Id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpDelete]
    [PermissionAuthorize(Consts.Permissions.TenantSettingsManage)]
    [Route(RouteWebVersion1.Secure.BookingConfig.Category.Delete)]
    public async Task<ApiResult> Delete(Guid id)
    {
        try
        {
            return await bookingConfigurationService.DeleteBookingCategoryAsync(id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting booking category {CategoryId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
}