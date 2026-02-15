using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookazone.Api.Controllers.V1.Config;
using Bookazone.Api.Controllers.V1.Config.Request;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Equipment;
using Bookazone.Application.Interfaces.Context;
using Bookazone.Application.Interfaces.Services.Equipment;
using Bookazone.Application.Interfaces.Services.Others;
using Bookazone.Domain.Enums;
using Bookazone.Infrastructure.Authorization;

namespace Bookazone.Api.Controllers.V1.Secure.Equipment;

[Authorize]
[ApiController]
[ApiExplorerSettings(GroupName = SwaggerConfig.SwaggerDocName.Equipment.Slug)]
[Route(RouteWebVersion1.Secure.Equipment.Base)]
public class EquipmentManagementController(
    IEquipmentManagementService equipmentManagementService,
    IFileStorageService fileStorageService,
    IUserContext userContext,
    ILogger<EquipmentManagementController> logger)
    : ControllerBase
{
    [HttpGet]
    [PermissionAuthorize(Consts.Permissions.ResourceView)]
    [Route(RouteWebVersion1.Secure.Equipment.All)]
    public async Task<ApiResult> All(
        [FromQuery] List<EquipmentStatus>? statuses = null,
        [FromQuery] string? category = null,
        [FromQuery] string? search = null,
        [FromQuery] string? city = null,
        [FromQuery] int? pageIndex = null,
        [FromQuery] int? pageSize = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = null)
    {
        try
        {
            return await equipmentManagementService.GetTenantEquipmentAsync(
                statuses,
                category,
                search,
                city,
                pageIndex,
                pageSize,
                sortBy,
                sortDirection);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting tenant equipment");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpGet]
    [PermissionAuthorize(Consts.Permissions.ResourceView)]
    [Route(RouteWebVersion1.Secure.Equipment.Find)]
    public async Task<ApiResult> Find([FromQuery] Guid id)
    {
        try
        {
            return await equipmentManagementService.GetEquipmentAsync(id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting equipment {EquipmentId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.ResourceCreate)]
    [Route(RouteWebVersion1.Secure.Equipment.Create)]
    public async Task<ApiResult> Create([FromBody] EquipmentCreateRequest request)
    {
        try
        {
            return await equipmentManagementService.CreateEquipmentAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating equipment for tenant {TenantId}", request.TenantId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPut]
    [PermissionAuthorize(Consts.Permissions.ResourceUpdate)]
    [Route(RouteWebVersion1.Secure.Equipment.Update)]
    public async Task<ApiResult> Update([FromBody] EquipmentUpdateRequest request)
    {
        try
        {
            return await equipmentManagementService.UpdateEquipmentAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating equipment {EquipmentId}", request.Id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.ResourceUpdate)]
    [Route(RouteWebVersion1.Secure.Equipment.Publish)]
    public async Task<ApiResult> Publish([FromBody] EquipmentPublishRequest request)
    {
        try
        {
            return await equipmentManagementService.PublishEquipmentAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing equipment {EquipmentId}", request.EquipmentId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.ResourceUpdate)]
    [Route(RouteWebVersion1.Secure.Equipment.Status)]
    public async Task<ApiResult> UpdateStatus([FromBody] EquipmentStatusUpdateRequest request)
    {
        try
        {
            return await equipmentManagementService.UpdateEquipmentStatusAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating equipment status {EquipmentId}", request.EquipmentId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpDelete]
    [PermissionAuthorize(Consts.Permissions.ResourceDelete)]
    [Route(RouteWebVersion1.Secure.Equipment.Delete)]
    public async Task<ApiResult> Delete([FromQuery] Guid id)
    {
        try
        {
            return await equipmentManagementService.DeleteEquipmentAsync(id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting equipment {EquipmentId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.ResourceUpdate)]
    [Route(RouteWebVersion1.Secure.Equipment.PricingBulk)]
    public async Task<ApiResult> UpsertPricing(
        [FromRoute] Guid equipmentId,
        [FromBody] List<EquipmentPricingRuleUpsertRequest> requests,
        [FromQuery] bool allOrNothing = false)
    {
        try
        {
            return await equipmentManagementService.UpsertEquipmentPricingRulesAsync(equipmentId, requests, allOrNothing);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error upserting pricing rules for equipment {EquipmentId}", equipmentId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.ResourceUpdate)]
    [Route(RouteWebVersion1.Secure.Equipment.SpecsBulk)]
    public async Task<ApiResult> UpsertSpecs(
        [FromRoute] Guid equipmentId,
        [FromBody] List<EquipmentSpecUpsertRequest> requests,
        [FromQuery] bool allOrNothing = false)
    {
        try
        {
            return await equipmentManagementService.UpsertEquipmentSpecsAsync(equipmentId, requests, allOrNothing);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error upserting specs for equipment {EquipmentId}", equipmentId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.ResourceUpdate)]
    [Route(RouteWebVersion1.Secure.Equipment.PoliciesBulk)]
    public async Task<ApiResult> UpsertPolicies(
        [FromRoute] Guid equipmentId,
        [FromBody] List<EquipmentPolicyUpsertRequest> requests,
        [FromQuery] bool allOrNothing = false)
    {
        try
        {
            return await equipmentManagementService.UpsertEquipmentPoliciesAsync(equipmentId, requests, allOrNothing);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error upserting policies for equipment {EquipmentId}", equipmentId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.ResourceUpdate)]
    [Route(RouteWebVersion1.Secure.Equipment.MediaBulk)]
    public async Task<ApiResult> UpsertMedia(
        [FromRoute] Guid equipmentId,
        [FromBody] List<EquipmentMediaUpsertRequest> requests,
        [FromQuery] bool allOrNothing = false)
    {
        try
        {
            return await equipmentManagementService.UpsertEquipmentMediaAsync(equipmentId, requests, allOrNothing);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error upserting media for equipment {EquipmentId}", equipmentId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.ResourceUpdate)]
    [Route(RouteWebVersion1.Secure.Equipment.MediaUpload)]
    public async Task<ApiResult> UploadMedia(
        [FromRoute] Guid equipmentId,
        [FromForm] EquipmentMediaUploadRequest request,
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
                FileCategory.EquipmentMedia,
                cancellationToken);

            var mediaRequest = new EquipmentMediaUpsertRequest
            {
                StoredFileId = storedFile.Id,
                IsCover = request.IsCover,
                SortOrder = request.SortOrder
            };

            return await equipmentManagementService.UpsertEquipmentMediaAsync(
                equipmentId,
                new List<EquipmentMediaUpsertRequest> { mediaRequest },
                false,
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error uploading media for equipment {EquipmentId}", equipmentId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
}
