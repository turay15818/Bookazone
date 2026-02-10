using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Converter;
using Bookazone.Application.DTOs.Events;
using Bookazone.Application.Interfaces.Repository.Events;
using Bookazone.Application.Interfaces.Services.Events;
using Bookazone.Domain.Entities.Events;

namespace Bookazone.Application.Services.Events;

public class EventCategoryService(
    IEventCategoryRepository eventCategoryRepository,
    ILogger<EventCategoryService> logger)
    : IEventCategoryService
{
    public async Task<ApiResult> GetEventCategoriesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var categories = await eventCategoryRepository.GetAllAsync();
            var dto = categories.Select(EventConverter.ToDto);
            return ApiResponse.Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting event categories");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> GetEventCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var category = await eventCategoryRepository.GetByIdAsync(id);
            return category == null
                ? ApiResponse.Error(ErrorHttp.NotFound)
                : ApiResponse.Success(EventConverter.ToDto(category));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error finding event category {CategoryId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> CreateEventCategoryAsync(EventCategoryCreateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Code))
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Name and code are required.");

            var normalizedName = NormalizeName(request.Name);
            var normalizedCode = NormalizeCode(request.Code);
            var nameLower = normalizedName.ToLowerInvariant();
            var codeLower = normalizedCode.ToLowerInvariant();

            var nameExists = await eventCategoryRepository.Query()
                .AsNoTracking()
                .AnyAsync(c => !c.Deleted && c.Name.ToLower() == nameLower, cancellationToken);

            if (nameExists)
                return ApiResponse.ErrorWithMessage(ErrorHttp.AlreadyExists, "A category with the same name already exists.");

            var codeExists = await eventCategoryRepository.Query()
                .AsNoTracking()
                .AnyAsync(c => !c.Deleted && c.Code.ToLower() == codeLower, cancellationToken);

            if (codeExists)
                return ApiResponse.ErrorWithMessage(ErrorHttp.AlreadyExists, "A category with the same code already exists.");

            var category = new EventCategory
            {
                Name = normalizedName,
                Code = normalizedCode,
                Description = request.Description?.Trim(),
                Active = request.Active,
                Deleted = false
            };

            var created = await eventCategoryRepository.AddAsync(category);
            return ApiResponse.Success(EventConverter.ToDto(created));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating event category");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> UpdateEventCategoryAsync(EventCategoryUpdateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await eventCategoryRepository.GetByIdAsync(request.Id);
            if (existing == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Code))
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Name and code are required.");

            var normalizedName = NormalizeName(request.Name);
            var normalizedCode = NormalizeCode(request.Code);
            var nameLower = normalizedName.ToLowerInvariant();
            var codeLower = normalizedCode.ToLowerInvariant();

            var nameExists = await eventCategoryRepository.Query()
                .AsNoTracking()
                .AnyAsync(c => !c.Deleted && c.Id != existing.Id && c.Name.ToLower() == nameLower, cancellationToken);

            if (nameExists)
                return ApiResponse.ErrorWithMessage(ErrorHttp.AlreadyExists, "A category with the same name already exists.");

            var codeExists = await eventCategoryRepository.Query()
                .AsNoTracking()
                .AnyAsync(c => !c.Deleted && c.Id != existing.Id && c.Code.ToLower() == codeLower, cancellationToken);

            if (codeExists)
                return ApiResponse.ErrorWithMessage(ErrorHttp.AlreadyExists, "A category with the same code already exists.");

            existing.Name = normalizedName;
            existing.Code = normalizedCode;
            existing.Description = request.Description?.Trim();
            existing.Active = request.Active;

            var updated = await eventCategoryRepository.UpdateAsync(existing);
            return ApiResponse.Success(EventConverter.ToDto(updated));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating event category {CategoryId}", request.Id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> DeleteEventCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var deleted = await eventCategoryRepository.DeleteAsync(id);
            return deleted
                ? ApiResponse.Success(true)
                : ApiResponse.Error(ErrorHttp.DbDeleteError);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting event category {CategoryId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    private static string NormalizeCode(string code) => code.Trim().ToUpperInvariant();

    private static string NormalizeName(string name) => name.Trim();
}
