using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookazone.Api.Controllers.V1.Config;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Reviews;
using Bookazone.Application.Interfaces.Services.Reviews;
using Bookazone.Domain.Enums;

namespace Bookazone.Api.Controllers.V1.Secure.Reviews;

[Authorize]
[ApiController]
[ApiExplorerSettings(GroupName = SwaggerConfig.SwaggerDocName.Reviews.Slug)]
[Route(RouteWebVersion1.Secure.Reviews.Base)]
public class ReviewsController(
    IReviewService reviewService,
    ILogger<ReviewsController> logger)
    : ControllerBase
{
    [HttpPost]
    [Route(RouteWebVersion1.Secure.Reviews.Create)]
    public async Task<ApiResult> Create([FromBody] ReviewCreateRequest request)
    {
        try
        {
            return await reviewService.CreateAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating review for {TargetType} {TargetId}", request.TargetType, request.TargetId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpGet]
    [Route(RouteWebVersion1.Secure.Reviews.Find)]
    public async Task<ApiResult> Find([FromQuery] Guid id)
    {
        try
        {
            return await reviewService.GetReviewAsync(id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting review {ReviewId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpGet]
    [Route(RouteWebVersion1.Secure.Reviews.Customer)]
    public async Task<ApiResult> Customer(
        [FromQuery] List<ReviewStatus>? statuses = null,
        [FromQuery] ReviewTargetType? targetType = null,
        [FromQuery] Guid? targetId = null,
        [FromQuery] int? pageIndex = null,
        [FromQuery] int? pageSize = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = null)
    {
        try
        {
            return await reviewService.GetCustomerReviewsAsync(
                statuses,
                targetType,
                targetId,
                pageIndex,
                pageSize,
                sortBy,
                sortDirection);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting customer reviews");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpGet]
    [Route(RouteWebVersion1.Secure.Reviews.Tenant)]
    public async Task<ApiResult> Tenant(
        [FromQuery] List<ReviewStatus>? statuses = null,
        [FromQuery] ReviewTargetType? targetType = null,
        [FromQuery] Guid? targetId = null,
        [FromQuery] int? pageIndex = null,
        [FromQuery] int? pageSize = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = null)
    {
        try
        {
            return await reviewService.GetTenantReviewsAsync(
                statuses,
                targetType,
                targetId,
                pageIndex,
                pageSize,
                sortBy,
                sortDirection);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting tenant reviews");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [Route(RouteWebVersion1.Secure.Reviews.Status)]
    public async Task<ApiResult> UpdateStatus([FromBody] ReviewStatusUpdateRequest request)
    {
        try
        {
            return await reviewService.UpdateStatusAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating review status {ReviewId}", request.ReviewId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [Route(RouteWebVersion1.Secure.Reviews.Reply)]
    public async Task<ApiResult> Reply([FromBody] ReviewReplyRequest request)
    {
        try
        {
            return await reviewService.ReplyAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error replying to review {ReviewId}", request.ReviewId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
}
