using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookazone.Api.Controllers.V1.Config;
using Bookazone.Api.Controllers.V1.Config.Request;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Rentals;
using Bookazone.Application.Interfaces.Context;
using Bookazone.Application.Interfaces.Services.Others;
using Bookazone.Application.Interfaces.Services.Rentals;
using Bookazone.Domain.Enums;
using Bookazone.Infrastructure.Authorization;

namespace Bookazone.Api.Controllers.V1.Secure.Rentals;

[Authorize]
[ApiController]
[ApiExplorerSettings(GroupName = SwaggerConfig.SwaggerDocName.Rentals.Slug)]
[Route(RouteWebVersion1.Secure.Rentals.Base)]
public class RentalManagementController(
    IRentalManagementService rentalManagementService,
    IFileStorageService fileStorageService,
    IUserContext userContext,
    ILogger<RentalManagementController> logger)
    : ControllerBase
{
    [HttpGet]
    [PermissionAuthorize(Consts.Permissions.ResourceView)]
    [Route(RouteWebVersion1.Secure.Rentals.All)]
    public async Task<ApiResult> All(
        [FromQuery] List<RentalStatus>? statuses = null,
        [FromQuery] RentalType? type = null,
        [FromQuery] string? search = null,
        [FromQuery] string? city = null,
        [FromQuery] int? pageIndex = null,
        [FromQuery] int? pageSize = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = null)
    {
        try
        {
            return await rentalManagementService.GetTenantRentalsAsync(
                statuses,
                type,
                search,
                city,
                pageIndex,
                pageSize,
                sortBy,
                sortDirection);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting tenant rentals");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpGet]
    [PermissionAuthorize(Consts.Permissions.ResourceView)]
    [Route(RouteWebVersion1.Secure.Rentals.Find)]
    public async Task<ApiResult> Find([FromQuery] Guid id)
    {
        try
        {
            return await rentalManagementService.GetRentalAsync(id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting rental {RentalId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.ResourceCreate)]
    [Route(RouteWebVersion1.Secure.Rentals.Create)]
    public async Task<ApiResult> Create([FromBody] RentalCreateRequest request)
    {
        try
        {
            return await rentalManagementService.CreateRentalAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating rental for tenant {TenantId}", request.TenantId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPut]
    [PermissionAuthorize(Consts.Permissions.ResourceUpdate)]
    [Route(RouteWebVersion1.Secure.Rentals.Update)]
    public async Task<ApiResult> Update([FromBody] RentalUpdateRequest request)
    {
        try
        {
            return await rentalManagementService.UpdateRentalAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating rental {RentalId}", request.Id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.ResourceUpdate)]
    [Route(RouteWebVersion1.Secure.Rentals.Publish)]
    public async Task<ApiResult> Publish([FromBody] RentalPublishRequest request)
    {
        try
        {
            return await rentalManagementService.PublishRentalAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing rental {RentalId}", request.RentalId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.ResourceUpdate)]
    [Route(RouteWebVersion1.Secure.Rentals.Status)]
    public async Task<ApiResult> UpdateStatus([FromBody] RentalStatusUpdateRequest request)
    {
        try
        {
            return await rentalManagementService.UpdateRentalStatusAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating rental status {RentalId}", request.RentalId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpDelete]
    [PermissionAuthorize(Consts.Permissions.ResourceDelete)]
    [Route(RouteWebVersion1.Secure.Rentals.Delete)]
    public async Task<ApiResult> Delete([FromQuery] Guid id)
    {
        try
        {
            return await rentalManagementService.DeleteRentalAsync(id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting rental {RentalId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.ResourceUpdate)]
    [Route(RouteWebVersion1.Secure.Rentals.PricingBulk)]
    public async Task<ApiResult> UpsertPricing(
        [FromRoute] Guid rentalId,
        [FromBody] List<RentalPricingRuleUpsertRequest> requests,
        [FromQuery] bool allOrNothing = false)
    {
        try
        {
            return await rentalManagementService.UpsertRentalPricingRulesAsync(rentalId, requests, allOrNothing);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error upserting pricing rules for rental {RentalId}", rentalId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.ResourceUpdate)]
    [Route(RouteWebVersion1.Secure.Rentals.SpecsBulk)]
    public async Task<ApiResult> UpsertSpecs(
        [FromRoute] Guid rentalId,
        [FromBody] List<RentalSpecUpsertRequest> requests,
        [FromQuery] bool allOrNothing = false)
    {
        try
        {
            return await rentalManagementService.UpsertRentalSpecsAsync(rentalId, requests, allOrNothing);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error upserting specs for rental {RentalId}", rentalId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.ResourceUpdate)]
    [Route(RouteWebVersion1.Secure.Rentals.PoliciesBulk)]
    public async Task<ApiResult> UpsertPolicies(
        [FromRoute] Guid rentalId,
        [FromBody] List<RentalPolicyUpsertRequest> requests,
        [FromQuery] bool allOrNothing = false)
    {
        try
        {
            return await rentalManagementService.UpsertRentalPoliciesAsync(rentalId, requests, allOrNothing);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error upserting policies for rental {RentalId}", rentalId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.ResourceUpdate)]
    [Route(RouteWebVersion1.Secure.Rentals.MediaBulk)]
    public async Task<ApiResult> UpsertMedia(
        [FromRoute] Guid rentalId,
        [FromBody] List<RentalMediaUpsertRequest> requests,
        [FromQuery] bool allOrNothing = false)
    {
        try
        {
            return await rentalManagementService.UpsertRentalMediaAsync(rentalId, requests, allOrNothing);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error upserting media for rental {RentalId}", rentalId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [PermissionAuthorize(Consts.Permissions.ResourceUpdate)]
    [Route(RouteWebVersion1.Secure.Rentals.MediaUpload)]
    public async Task<ApiResult> UploadMedia(
        [FromRoute] Guid rentalId,
        [FromForm] RentalMediaUploadRequest request,
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
                FileCategory.RentalMedia,
                cancellationToken);

            var mediaRequest = new RentalMediaUpsertRequest
            {
                StoredFileId = storedFile.Id,
                IsCover = request.IsCover,
                SortOrder = request.SortOrder
            };

            return await rentalManagementService.UpsertRentalMediaAsync(
                rentalId,
                new List<RentalMediaUpsertRequest> { mediaRequest },
                false,
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error uploading media for rental {RentalId}", rentalId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
}
