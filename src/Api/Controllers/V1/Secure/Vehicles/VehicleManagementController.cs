using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookazone.Api.Controllers.V1.Config;
using Bookazone.Api.Controllers.V1.Config.Request;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.Interfaces.Context;
using Bookazone.Application.Interfaces.Services.Others;
using Bookazone.Application.Interfaces.Services.Vehicles;
using Bookazone.Application.DTOs.Vehicles;
using Bookazone.Domain.Enums;
using Bookazone.Infrastructure.Authorization;

namespace Bookazone.Api.Controllers.V1.Secure.Vehicles;

[Authorize]
[ApiController]
[ApiExplorerSettings(GroupName = SwaggerConfig.SwaggerDocName.Vehicles.Slug)]
[Route(RouteWebVersion1.Secure.Vehicles.Base)]
public class VehicleManagementController(
    IVehicleManagementService vehicleManagementService,
    IFileStorageService fileStorageService,
    IUserContext userContext,
    ILogger<VehicleManagementController> logger)
    : ControllerBase
{
    [HttpGet]
    [PermissionAuthorize(Consts.Permissions.ResourceView)]
    [Route(RouteWebVersion1.Secure.Vehicles.All)]
    public async Task<ApiResult> All(
        [FromQuery] List<VehicleStatus>? statuses = null,
        [FromQuery] VehicleServiceType? serviceType = null,
        [FromQuery] VehicleType? vehicleType = null,
        [FromQuery] string? search = null,
        [FromQuery] string? city = null,
        [FromQuery] int? pageIndex = null,
        [FromQuery] int? pageSize = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = null)
    {
        try
        {
            return await vehicleManagementService.GetTenantVehiclesAsync(
                statuses,
                serviceType,
                vehicleType,
                search,
                city,
                pageIndex,
                pageSize,
                sortBy,
                sortDirection);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting tenant vehicles");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpGet]
    [PermissionAuthorize(Consts.Permissions.ResourceView)]
    [Route(RouteWebVersion1.Secure.Vehicles.Find)]
    public async Task<ApiResult> Find([FromQuery] Guid id)
    {
        try
        {
            return await vehicleManagementService.GetVehicleAsync(id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting vehicle {VehicleId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.ResourceCreate)]
    [Route(RouteWebVersion1.Secure.Vehicles.Create)]
    public async Task<ApiResult> Create([FromBody] VehicleCreateRequest request)
    {
        try
        {
            return await vehicleManagementService.CreateVehicleAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating vehicle for tenant {TenantId}", request.TenantId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPut]
    [PermissionAuthorize(Consts.Permissions.ResourceUpdate)]
    [Route(RouteWebVersion1.Secure.Vehicles.Update)]
    public async Task<ApiResult> Update([FromBody] VehicleUpdateRequest request)
    {
        try
        {
            return await vehicleManagementService.UpdateVehicleAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating vehicle {VehicleId}", request.Id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.ResourceUpdate)]
    [Route(RouteWebVersion1.Secure.Vehicles.Publish)]
    public async Task<ApiResult> Publish([FromBody] VehiclePublishRequest request)
    {
        try
        {
            return await vehicleManagementService.PublishVehicleAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing vehicle {VehicleId}", request.VehicleId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.ResourceUpdate)]
    [Route(RouteWebVersion1.Secure.Vehicles.Status)]
    public async Task<ApiResult> UpdateStatus([FromBody] VehicleStatusUpdateRequest request)
    {
        try
        {
            return await vehicleManagementService.UpdateVehicleStatusAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating vehicle status {VehicleId}", request.VehicleId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpDelete]
    [PermissionAuthorize(Consts.Permissions.ResourceDelete)]
    [Route(RouteWebVersion1.Secure.Vehicles.Delete)]
    public async Task<ApiResult> Delete([FromQuery] Guid id)
    {
        try
        {
            return await vehicleManagementService.DeleteVehicleAsync(id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting vehicle {VehicleId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.ResourceUpdate)]
    [Route(RouteWebVersion1.Secure.Vehicles.PricingBulk)]
    public async Task<ApiResult> UpsertPricing(
        [FromRoute] Guid vehicleId,
        [FromBody] List<VehiclePricingRuleUpsertRequest> requests,
        [FromQuery] bool allOrNothing = false)
    {
        try
        {
            return await vehicleManagementService.UpsertVehiclePricingRulesAsync(vehicleId, requests, allOrNothing);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error upserting pricing rules for vehicle {VehicleId}", vehicleId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.ResourceUpdate)]
    [Route(RouteWebVersion1.Secure.Vehicles.SpecsBulk)]
    public async Task<ApiResult> UpsertSpecs(
        [FromRoute] Guid vehicleId,
        [FromBody] List<VehicleSpecUpsertRequest> requests,
        [FromQuery] bool allOrNothing = false)
    {
        try
        {
            return await vehicleManagementService.UpsertVehicleSpecsAsync(vehicleId, requests, allOrNothing);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error upserting specs for vehicle {VehicleId}", vehicleId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.ResourceUpdate)]
    [Route(RouteWebVersion1.Secure.Vehicles.PoliciesBulk)]
    public async Task<ApiResult> UpsertPolicies(
        [FromRoute] Guid vehicleId,
        [FromBody] List<VehiclePolicyUpsertRequest> requests,
        [FromQuery] bool allOrNothing = false)
    {
        try
        {
            return await vehicleManagementService.UpsertVehiclePoliciesAsync(vehicleId, requests, allOrNothing);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error upserting policies for vehicle {VehicleId}", vehicleId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.ResourceUpdate)]
    [Route(RouteWebVersion1.Secure.Vehicles.MediaBulk)]
    public async Task<ApiResult> UpsertMedia(
        [FromRoute] Guid vehicleId,
        [FromBody] List<VehicleMediaUpsertRequest> requests,
        [FromQuery] bool allOrNothing = false)
    {
        try
        {
            return await vehicleManagementService.UpsertVehicleMediaAsync(vehicleId, requests, allOrNothing);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error upserting media for vehicle {VehicleId}", vehicleId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.ResourceUpdate)]
    [Route(RouteWebVersion1.Secure.Vehicles.MediaUpload)]
    public async Task<ApiResult> UploadMedia(
        [FromRoute] Guid vehicleId,
        [FromForm] VehicleMediaUploadRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (request.File == null)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "File is required.");

            if (!userContext.TenantId.HasValue || !userContext.UserId.HasValue)
                return ApiResponse.Error(ErrorHttp.Forbidden,
                    new Exception("User context does not contain a valid tenant or user id."));

            var storedFile = await fileStorageService.UploadAsync(
                request.File,
                userContext.TenantId.Value,
                userContext.UserId.Value,
                FileCategory.VehicleMedia,
                cancellationToken);

            var mediaRequest = new VehicleMediaUpsertRequest
            {
                StoredFileId = storedFile.Id,
                IsCover = request.IsCover,
                SortOrder = request.SortOrder
            };

            return await vehicleManagementService.UpsertVehicleMediaAsync(
                vehicleId,
                new List<VehicleMediaUpsertRequest> { mediaRequest },
                false,
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error uploading media for vehicle {VehicleId}", vehicleId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
}
