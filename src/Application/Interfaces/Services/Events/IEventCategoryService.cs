using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Events;

namespace Bookazone.Application.Interfaces.Services.Events;

public interface IEventCategoryService
{
    Task<ApiResult> GetEventCategoriesAsync(CancellationToken cancellationToken = default);
    Task<ApiResult> GetEventCategoryAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResult> CreateEventCategoryAsync(EventCategoryCreateRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> UpdateEventCategoryAsync(EventCategoryUpdateRequest request, CancellationToken cancellationToken = default);
    Task<ApiResult> DeleteEventCategoryAsync(Guid id, CancellationToken cancellationToken = default);
}
