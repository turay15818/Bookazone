using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Reviews;
using Bookazone.Domain.Enums;

namespace Bookazone.Application.Interfaces.Services.Reviews;

public interface IReviewService
{
    Task<ApiResult> CreateAsync(ReviewCreateRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> GetReviewAsync(Guid reviewId, CancellationToken cancellationToken = default);
    Task<ApiResult> GetPublicReviewsAsync(
        ReviewTargetType targetType,
        Guid targetId,
        int? pageIndex = null,
        int? pageSize = null,
        string? sortBy = null,
        string? sortDirection = null,
        CancellationToken cancellationToken = default);
    Task<ApiResult> GetCustomerReviewsAsync(
        List<ReviewStatus>? statuses = null,
        ReviewTargetType? targetType = null,
        Guid? targetId = null,
        int? pageIndex = null,
        int? pageSize = null,
        string? sortBy = null,
        string? sortDirection = null,
        CancellationToken cancellationToken = default);
    Task<ApiResult> GetTenantReviewsAsync(
        List<ReviewStatus>? statuses = null,
        ReviewTargetType? targetType = null,
        Guid? targetId = null,
        int? pageIndex = null,
        int? pageSize = null,
        string? sortBy = null,
        string? sortDirection = null,
        CancellationToken cancellationToken = default);
    Task<ApiResult> GetSummaryAsync(ReviewTargetType targetType, Guid targetId, CancellationToken cancellationToken = default);
    Task<ApiResult> UpdateStatusAsync(ReviewStatusUpdateRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> ReplyAsync(ReviewReplyRequest request, CancellationToken cancellationToken = default);
}
