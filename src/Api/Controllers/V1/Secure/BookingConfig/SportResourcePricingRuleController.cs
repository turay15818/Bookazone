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
[Route(RouteWebVersion1.Secure.BookingConfig.SportResourcePricing.Base)]
public class SportResourcePricingRuleController(
    IBookingConfigurationService bookingConfigurationService,
    ILogger<SportResourcePricingRuleController> logger)
    : ControllerBase
{
    [HttpGet]
   // [PermissionAuthorize(Consts.Permissions.ResourceView)]
    [Route(RouteWebVersion1.Secure.BookingConfig.SportResourcePricing.All)]
    public async Task<ApiResult> All(Guid resourceId)
    {
        try
        {
            return await bookingConfigurationService.GetSportResourcePricingRulesAsync(resourceId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting sport resource pricing rules for resource {ResourceId}", resourceId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.ResourceCreate)]
    [Route(RouteWebVersion1.Secure.BookingConfig.SportResourcePricing.Create)]
    public async Task<ApiResult> Create([FromBody] SportResourcePricingRuleCreateRequest request)
    {
        try
        {
            return await bookingConfigurationService.CreateSportResourcePricingRuleAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating sport resource pricing rule for resource {ResourceId}", request.ResourceId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPut]
    [PermissionAuthorize(Consts.Permissions.ResourceUpdate)]
    [Route(RouteWebVersion1.Secure.BookingConfig.SportResourcePricing.Update)]
    public async Task<ApiResult> Update([FromBody] SportResourcePricingRuleUpdateRequest request)
    {
        try
        {
            return await bookingConfigurationService.UpdateSportResourcePricingRuleAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating sport resource pricing rule {RuleId}", request.Id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpDelete]
    [PermissionAuthorize(Consts.Permissions.ResourceDelete)]
    [Route(RouteWebVersion1.Secure.BookingConfig.SportResourcePricing.Delete)]
    public async Task<ApiResult> Delete(Guid id)
    {
        try
        {
            return await bookingConfigurationService.DeleteSportResourcePricingRuleAsync(id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting sport resource pricing rule {RuleId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.ResourceCreate)]
    [Route(RouteWebVersion1.Secure.BookingConfig.SportResourcePricing.Bulk)]
    public async Task<ApiResult> Bulk(
        [FromBody] List<SportResourcePricingRuleUpsertRequest> requests,
        [FromQuery] bool allOrNothing = false)
    {
        try
        {
            return await bookingConfigurationService.UpsertSportResourcePricingRulesAsync(requests, allOrNothing);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error bulk upserting sport resource pricing rules");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
}