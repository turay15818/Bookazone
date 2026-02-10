using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookazone.Api.Controllers.V1.Config;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.Interfaces.Services.Reviews;
using Bookazone.Domain.Enums;

namespace Bookazone.Api.Controllers.V1.Public;

[AllowAnonymous]
[ApiController]
[ApiExplorerSettings(GroupName = SwaggerConfig.SwaggerDocName.Reviews.Slug)]
[Route(RouteWebVersion1.Public.Reviews.Base)]
public class ReviewsController(
    IReviewService reviewService,
    ILogger<ReviewsController> logger)
    : ControllerBase
{
    [HttpGet]
    [Route(RouteWebVersion1.Public.Reviews.All)]
    public async Task<ApiResult> All(
        [FromQuery] ReviewTargetType targetType,
        [FromQuery] Guid targetId,
        [FromQuery] int? pageIndex = null,
        [FromQuery] int? pageSize = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = null)
    {
        try
        {
            return await reviewService.GetPublicReviewsAsync(
                targetType,
                targetId,
                pageIndex,
                pageSize,
                sortBy,
                sortDirection);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting public reviews for {TargetType} {TargetId}", targetType, targetId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpGet]
    [Route(RouteWebVersion1.Public.Reviews.Summary)]
    public async Task<ApiResult> Summary(
        [FromQuery] ReviewTargetType targetType,
        [FromQuery] Guid targetId)
    {
        try
        {
            return await reviewService.GetSummaryAsync(targetType, targetId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting review summary for {TargetType} {TargetId}", targetType, targetId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
}
