using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Bookazone.Application.DTOs.Converter;
using Bookazone.Api.Controllers.V1.Config.Request;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.Interfaces.Repository.Booking;
using Bookazone.Application.Interfaces.Repository.Sports;
using Bookazone.Application.Interfaces.Repository.Tenant;
using Bookazone.Application.Interfaces.Services.Booking;
using Bookazone.Domain.Entities.Booking;
using Bookazone.Domain.Entities.Sports;
using Bookazone.Domain.Entities.Tenant;

namespace Bookazone.Application.Services.Booking;

public class BookingConfigurationService(
    ITenantRepository tenantRepository,
    IBookingCategoryRepository bookingCategoryRepository,
    ITenantBookingCategoryRepository tenantBookingCategoryRepository,
    ITenantWorkingHourRepository tenantWorkingHourRepository,
    ITenantSportTypeRepository tenantSportTypeRepository,
    ISportResourceRepository sportResourceRepository,
    ISportResourceAvailabilityRepository sportResourceAvailabilityRepository,
    ISportResourceBlackoutRepository sportResourceBlackoutRepository,
    ISportResourcePricingRuleRepository sportResourcePricingRuleRepository,
    ILogger<BookingConfigurationService> logger)
    : IBookingConfigurationService
{
    public async Task<ApiResult> GetBookingCategoriesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var categories = await bookingCategoryRepository.GetAllAsync();
            var dto = categories.Select(BookingConfigConverter.ToDto);
            return ApiResponse.Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting booking categories");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> GetBookingCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var category = await bookingCategoryRepository.GetByIdAsync(id);
            return category == null
                ? ApiResponse.Error(ErrorHttp.NotFound)
                : ApiResponse.Success(BookingConfigConverter.ToDto(category));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error finding booking category {CategoryId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> CreateBookingCategoryAsync(BookingCategoryCreateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Code))
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Name and code are required.");

            var normalizedName = NormalizeName(request.Name);
            var normalizedCode = NormalizeCode(request.Code);
            var nameLower = normalizedName.ToLowerInvariant();
            var codeLower = normalizedCode.ToLowerInvariant();

            var nameExists = await bookingCategoryRepository.Query()
                .AsNoTracking()
                .AnyAsync(c => !c.Deleted && c.Name.ToLower() == nameLower, cancellationToken);

            if (nameExists)
                return ApiResponse.ErrorWithMessage(ErrorHttp.AlreadyExists, "A category with the same name already exists.");

            var codeExists = await bookingCategoryRepository.Query()
                .AsNoTracking()
                .AnyAsync(c => !c.Deleted && c.Code.ToLower() == codeLower, cancellationToken);

            if (codeExists)
                return ApiResponse.ErrorWithMessage(ErrorHttp.AlreadyExists, "A category with the same code already exists.");

            var category = new BookingCategory
            {
                Name = normalizedName,
                Code = normalizedCode,
                Description = request.Description?.Trim(),
                DefaultBookingMode = request.DefaultBookingMode,
                Active = true,
                Deleted = false
            };

            var created = await bookingCategoryRepository.AddAsync(category);
            return ApiResponse.Success(BookingConfigConverter.ToDto(created));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating booking category");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> UpdateBookingCategoryAsync(BookingCategoryUpdateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await bookingCategoryRepository.GetByIdAsync(request.Id);
            if (existing == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Code))
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Name and code are required.");

            var normalizedName = NormalizeName(request.Name);
            var normalizedCode = NormalizeCode(request.Code);
            var nameLower = normalizedName.ToLowerInvariant();
            var codeLower = normalizedCode.ToLowerInvariant();

            var nameExists = await bookingCategoryRepository.Query()
                .AsNoTracking()
                .AnyAsync(c => !c.Deleted && c.Id != existing.Id && c.Name.ToLower() == nameLower, cancellationToken);

            if (nameExists)
                return ApiResponse.ErrorWithMessage(ErrorHttp.AlreadyExists, "A category with the same name already exists.");

            var codeExists = await bookingCategoryRepository.Query()
                .AsNoTracking()
                .AnyAsync(c => !c.Deleted && c.Id != existing.Id && c.Code.ToLower() == codeLower, cancellationToken);

            if (codeExists)
                return ApiResponse.ErrorWithMessage(ErrorHttp.AlreadyExists, "A category with the same code already exists.");

            existing.Name = normalizedName;
            existing.Code = normalizedCode;
            existing.Description = request.Description?.Trim();
            existing.DefaultBookingMode = request.DefaultBookingMode;

            var updated = await bookingCategoryRepository.UpdateAsync(existing);
            return ApiResponse.Success(BookingConfigConverter.ToDto(updated));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating booking category {CategoryId}", request.Id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> DeleteBookingCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var deleted = await bookingCategoryRepository.DeleteAsync(id);
            return deleted
                ? ApiResponse.Success(true)
                : ApiResponse.Error(ErrorHttp.DbDeleteError);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting booking category {CategoryId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> GetTenantBookingCategoriesAsync(Guid tenantId, bool enabledOnly = false, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await tenantRepository.ExistsAsync(tenantId))
                return ApiResponse.Error(ErrorHttp.NotFound);

            var categories = enabledOnly
                ? await tenantBookingCategoryRepository.GetEnabledByTenantAsync(tenantId, cancellationToken)
                : await tenantBookingCategoryRepository.GetByTenantAsync(tenantId, cancellationToken);

            var dto = categories.Select(BookingConfigConverter.ToDto);
            return ApiResponse.Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting tenant booking categories for {TenantId}", tenantId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> UpsertTenantBookingCategoryAsync(TenantBookingCategoryUpsertRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await tenantRepository.ExistsAsync(request.TenantId))
                return ApiResponse.Error(ErrorHttp.NotFound);

            var category = await bookingCategoryRepository.GetByIdAsync(request.BookingCategoryId);
            if (category == null || !category.Active)
                return ApiResponse.ErrorWithMessage(ErrorHttp.NotFound, "Booking category not found or inactive.");

            var existing = await tenantBookingCategoryRepository.GetByTenantAndCategoryAsync(
                request.TenantId,
                request.BookingCategoryId,
                cancellationToken);

                if (existing != null)
                {
                    existing.IsEnabled = request.IsEnabled;
                    existing.BookingModeOverride = request.BookingModeOverride;
                    var updated = await tenantBookingCategoryRepository.UpdateAsync(existing);
                    return ApiResponse.Success(BookingConfigConverter.ToDto(updated, category));
                }

            var tenantCategory = new TenantBookingCategory
            {
                FkTenantId = request.TenantId,
                FkBookingCategoryId = request.BookingCategoryId,
                IsEnabled = request.IsEnabled,
                BookingModeOverride = request.BookingModeOverride,
                Active = true,
                Deleted = false
            };

            var created = await tenantBookingCategoryRepository.AddAsync(tenantCategory);
            return ApiResponse.Success(BookingConfigConverter.ToDto(created, category));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error upserting tenant booking category for tenant {TenantId}", request.TenantId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> UpsertTenantBookingCategoriesAsync(
        List<TenantBookingCategoryUpsertRequest> requests,
        bool allOrNothing = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (requests == null || requests.Count == 0)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Request list is empty.");

            await using IDbContextTransaction? transaction = allOrNothing
                ? await tenantBookingCategoryRepository.BeginTransactionAsync()
                : null;

            var results = new List<object>();
            var successCount = 0;
            var failureCount = 0;

            foreach (var request in requests)
            {
                var result = await UpsertTenantBookingCategoryAsync(request, cancellationToken);

                if (result is ApiResultSuccess success)
                {
                    successCount++;
                    results.Add(new
                    {
                        request.TenantId,
                        request.BookingCategoryId,
                        Success = true,
                        Message = success.Message,
                        Data = success.Data
                    });
                    continue;
                }

                failureCount++;
                results.Add(new
                {
                    request.TenantId,
                    request.BookingCategoryId,
                    Success = false,
                    Message = result.Message
                });

                if (allOrNothing)
                {
                    if (transaction != null)
                        await transaction.RollbackAsync(cancellationToken);

                    return ApiResponse.ErrorWithMessage(
                        ErrorHttp.BadRequest,
                        $"Bulk upsert failed for tenant {request.TenantId} and category {request.BookingCategoryId}: {result.Message}");
                }
            }

            if (allOrNothing && transaction != null)
                await transaction.CommitAsync(cancellationToken);

            return ApiResponse.Success(new
            {
                Total = results.Count,
                Succeeded = successCount,
                Failed = failureCount,
                Results = results
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error bulk upserting tenant booking categories");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> GetTenantWorkingHoursAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await tenantRepository.ExistsAsync(tenantId))
                return ApiResponse.Error(ErrorHttp.NotFound);

            var hours = await tenantWorkingHourRepository.GetByTenantAsync(tenantId, cancellationToken);
            var dto = hours.Select(BookingConfigConverter.ToDto);
            return ApiResponse.Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting tenant working hours for {TenantId}", tenantId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> UpsertTenantWorkingHourAsync(TenantWorkingHourUpsertRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await tenantRepository.ExistsAsync(request.TenantId))
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (request.SlotDurationMinutes <= 0)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Slot duration must be greater than zero.");

            if (!request.IsClosed)
            {
                if (request.OpenTime == null || request.CloseTime == null)
                    return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Open and close time are required when the tenant is open.");

                if (!IsTimeInDay(request.OpenTime.Value) || !IsTimeInDay(request.CloseTime.Value))
                    return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Open and close time must be within a single day.");

                if (request.OpenTime >= request.CloseTime)
                    return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Open time must be before close time.");
            }

            var workingHour = new TenantWorkingHour
            {
                FkTenantId = request.TenantId,
                DayOfWeek = request.DayOfWeek,
                OpenTime = request.IsClosed ? null : request.OpenTime,
                CloseTime = request.IsClosed ? null : request.CloseTime,
                IsClosed = request.IsClosed,
                SlotDurationMinutes = request.SlotDurationMinutes,
                Active = true,
                Deleted = false
            };

            var saved = await tenantWorkingHourRepository.CreateOrUpdateAsync(workingHour, cancellationToken);
            return ApiResponse.Success(BookingConfigConverter.ToDto(saved));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error upserting tenant working hour for tenant {TenantId}", request.TenantId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> UpsertTenantWorkingHoursAsync(
        List<TenantWorkingHourUpsertRequest> requests,
        bool allOrNothing = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (requests == null || requests.Count == 0)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Request list is empty.");

            await using IDbContextTransaction? transaction = allOrNothing
                ? await tenantWorkingHourRepository.BeginTransactionAsync()
                : null;

            var results = new List<object>();
            var successCount = 0;
            var failureCount = 0;

            foreach (var request in requests)
            {
                var result = await UpsertTenantWorkingHourAsync(request, cancellationToken);

                if (result is ApiResultSuccess success)
                {
                    successCount++;
                    results.Add(new
                    {
                        request.TenantId,
                        request.DayOfWeek,
                        Success = true,
                        Message = success.Message,
                        Data = success.Data
                    });
                    continue;
                }

                failureCount++;
                results.Add(new
                {
                    request.TenantId,
                    request.DayOfWeek,
                    Success = false,
                    Message = result.Message
                });

                if (allOrNothing)
                {
                    if (transaction != null)
                        await transaction.RollbackAsync(cancellationToken);

                    return ApiResponse.ErrorWithMessage(
                        ErrorHttp.BadRequest,
                        $"Bulk upsert failed for tenant {request.TenantId} and day {request.DayOfWeek}: {result.Message}");
                }
            }

            if (allOrNothing && transaction != null)
                await transaction.CommitAsync(cancellationToken);

            return ApiResponse.Success(new
            {
                Total = results.Count,
                Succeeded = successCount,
                Failed = failureCount,
                Results = results
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error bulk upserting tenant working hours");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> GetTenantSportTypesAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await tenantRepository.ExistsAsync(tenantId))
                return ApiResponse.Error(ErrorHttp.NotFound);

            var types = await tenantSportTypeRepository.GetByTenantAsync(tenantId, cancellationToken);
            var dto = types.Select(BookingConfigConverter.ToDto);
            return ApiResponse.Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting tenant sport types for {TenantId}", tenantId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> CreateTenantSportTypeAsync(TenantSportTypeCreateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await tenantRepository.ExistsAsync(request.TenantId))
                return ApiResponse.Error(ErrorHttp.NotFound);

            var tenantCategory = await tenantBookingCategoryRepository.GetByIdAsync(request.TenantBookingCategoryId);
            if (tenantCategory == null || tenantCategory.FkTenantId != request.TenantId)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Tenant booking category is invalid for this tenant.");

            var validation = ValidateDuration(request.MinDurationMinutes, request.MaxDurationMinutes, request.BufferMinutes);
            if (validation != null)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, validation);

            var existing = await tenantSportTypeRepository.GetByTenantAndNameAsync(request.TenantId, request.Name, cancellationToken);
            if (existing != null)
                return ApiResponse.ErrorWithMessage(ErrorHttp.AlreadyExists, "A sport type with the same name already exists.");

            var sportType = new TenantSportType
            {
                FkTenantId = request.TenantId,
                FkTenantBookingCategoryId = request.TenantBookingCategoryId,
                Name = NormalizeName(request.Name),
                Description = request.Description?.Trim(),
                BookingMode = request.BookingMode,
                MinDurationMinutes = request.MinDurationMinutes,
                MaxDurationMinutes = request.MaxDurationMinutes,
                BufferMinutes = request.BufferMinutes,
                Active = true,
                Deleted = false
            };

            var created = await tenantSportTypeRepository.AddAsync(sportType);
            return ApiResponse.Success(BookingConfigConverter.ToDto(created));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating tenant sport type for tenant {TenantId}", request.TenantId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> UpdateTenantSportTypeAsync(TenantSportTypeUpdateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await tenantSportTypeRepository.GetByIdAsync(request.Id);
            if (existing == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (existing.FkTenantId != request.TenantId)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Tenant mismatch for sport type update.");

            var tenantCategory = await tenantBookingCategoryRepository.GetByIdAsync(request.TenantBookingCategoryId);
            if (tenantCategory == null || tenantCategory.FkTenantId != request.TenantId)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Tenant booking category is invalid for this tenant.");

            var validation = ValidateDuration(request.MinDurationMinutes, request.MaxDurationMinutes, request.BufferMinutes);
            if (validation != null)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, validation);

            var duplicate = await tenantSportTypeRepository.GetByTenantAndNameAsync(request.TenantId, request.Name, cancellationToken);
            if (duplicate != null && duplicate.Id != existing.Id)
                return ApiResponse.ErrorWithMessage(ErrorHttp.AlreadyExists, "A sport type with the same name already exists.");

            existing.FkTenantBookingCategoryId = request.TenantBookingCategoryId;
            existing.Name = NormalizeName(request.Name);
            existing.Description = request.Description?.Trim();
            existing.BookingMode = request.BookingMode;
            existing.MinDurationMinutes = request.MinDurationMinutes;
            existing.MaxDurationMinutes = request.MaxDurationMinutes;
            existing.BufferMinutes = request.BufferMinutes;

            var updated = await tenantSportTypeRepository.UpdateAsync(existing);
            return ApiResponse.Success(BookingConfigConverter.ToDto(updated));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating tenant sport type {SportTypeId}", request.Id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> DeleteTenantSportTypeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var deleted = await tenantSportTypeRepository.DeleteAsync(id);
            return deleted
                ? ApiResponse.Success(true)
                : ApiResponse.Error(ErrorHttp.DbDeleteError);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting tenant sport type {SportTypeId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> GetSportResourcesByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await tenantRepository.ExistsAsync(tenantId))
                return ApiResponse.Error(ErrorHttp.NotFound);

            var resources = await sportResourceRepository.GetByTenantAsync(tenantId, cancellationToken);
            var dto = resources.Select(BookingConfigConverter.ToDto);
            return ApiResponse.Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting sport resources for tenant {TenantId}", tenantId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> GetSportResourcesByTypeAsync(Guid tenantSportTypeId, CancellationToken cancellationToken = default)
    {
        try
        {
            var resources = await sportResourceRepository.GetBySportTypeAsync(tenantSportTypeId, cancellationToken);
            var dto = resources.Select(BookingConfigConverter.ToDto);
            return ApiResponse.Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting sport resources for sport type {SportTypeId}", tenantSportTypeId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> CreateSportResourceAsync(SportResourceCreateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await tenantRepository.ExistsAsync(request.TenantId))
                return ApiResponse.Error(ErrorHttp.NotFound);

            var sportType = await tenantSportTypeRepository.GetByIdAsync(request.TenantSportTypeId);
            if (sportType == null || sportType.FkTenantId != request.TenantId)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Sport type is invalid for this tenant.");

            var resource = new SportResource
            {
                FkTenantId = request.TenantId,
                FkTenantSportTypeId = request.TenantSportTypeId,
                Name = NormalizeName(request.Name),
                Description = request.Description?.Trim(),
                Capacity = request.Capacity,
                Address = request.Address?.Trim(),
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Active = true,
                Deleted = false
            };

            var created = await sportResourceRepository.AddAsync(resource);
            return ApiResponse.Success(BookingConfigConverter.ToDto(created));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating sport resource for tenant {TenantId}", request.TenantId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> UpdateSportResourceAsync(SportResourceUpdateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await sportResourceRepository.GetByIdAsync(request.Id);
            if (existing == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (existing.FkTenantId != request.TenantId)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Tenant mismatch for resource update.");

            var sportType = await tenantSportTypeRepository.GetByIdAsync(request.TenantSportTypeId);
            if (sportType == null || sportType.FkTenantId != request.TenantId)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Sport type is invalid for this tenant.");

            existing.FkTenantSportTypeId = request.TenantSportTypeId;
            existing.Name = NormalizeName(request.Name);
            existing.Description = request.Description?.Trim();
            existing.Capacity = request.Capacity;
            existing.Address = request.Address?.Trim();
            existing.Latitude = request.Latitude;
            existing.Longitude = request.Longitude;

            var updated = await sportResourceRepository.UpdateAsync(existing);
            return ApiResponse.Success(BookingConfigConverter.ToDto(updated));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating sport resource {ResourceId}", request.Id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> DeleteSportResourceAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var deleted = await sportResourceRepository.DeleteAsync(id);
            return deleted
                ? ApiResponse.Success(true)
                : ApiResponse.Error(ErrorHttp.DbDeleteError);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting sport resource {ResourceId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> GetSportResourceAvailabilitiesAsync(Guid resourceId, CancellationToken cancellationToken = default)
    {
        try
        {
            var resource = await sportResourceRepository.GetByIdAsync(resourceId);
            if (resource == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            var availability = await sportResourceAvailabilityRepository.GetByResourceAsync(resourceId, cancellationToken);
            var dto = availability.Select(BookingConfigConverter.ToDto);
            return ApiResponse.Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting sport resource availability for resource {ResourceId}", resourceId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> UpsertSportResourceAvailabilityAsync(SportResourceAvailabilityUpsertRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var resource = await sportResourceRepository.GetByIdAsync(request.ResourceId);
            if (resource == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (!request.IsClosed)
            {
                if (request.OpenTime == null || request.CloseTime == null)
                    return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Open and close time are required when the resource is open.");

                if (!IsTimeInDay(request.OpenTime.Value) || !IsTimeInDay(request.CloseTime.Value))
                    return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Open and close time must be within a single day.");

                if (request.OpenTime >= request.CloseTime)
                    return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Open time must be before close time.");
            }

            var availability = new SportResourceAvailability
            {
                FkSportResourceId = request.ResourceId,
                DayOfWeek = request.DayOfWeek,
                OpenTime = request.IsClosed ? null : request.OpenTime,
                CloseTime = request.IsClosed ? null : request.CloseTime,
                IsClosed = request.IsClosed,
                Active = true,
                Deleted = false
            };

            var saved = await sportResourceAvailabilityRepository.CreateOrUpdateAsync(availability, cancellationToken);
            return ApiResponse.Success(BookingConfigConverter.ToDto(saved));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error upserting sport resource availability for resource {ResourceId}", request.ResourceId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> UpsertSportResourceAvailabilitiesAsync(
        List<SportResourceAvailabilityUpsertRequest> requests,
        bool allOrNothing = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (requests == null || requests.Count == 0)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Request list is empty.");

            await using IDbContextTransaction? transaction = allOrNothing
                ? await sportResourceAvailabilityRepository.BeginTransactionAsync()
                : null;

            var results = new List<object>();
            var successCount = 0;
            var failureCount = 0;

            foreach (var request in requests)
            {
                var result = await UpsertSportResourceAvailabilityAsync(request, cancellationToken);

                if (result is ApiResultSuccess success)
                {
                    successCount++;
                    results.Add(new
                    {
                        request.ResourceId,
                        request.DayOfWeek,
                        Success = true,
                        Message = success.Message,
                        Data = success.Data
                    });
                    continue;
                }

                failureCount++;
                results.Add(new
                {
                    request.ResourceId,
                    request.DayOfWeek,
                    Success = false,
                    Message = result.Message
                });

                if (allOrNothing)
                {
                    if (transaction != null)
                        await transaction.RollbackAsync(cancellationToken);

                    return ApiResponse.ErrorWithMessage(
                        ErrorHttp.BadRequest,
                        $"Bulk upsert failed for resource {request.ResourceId} and day {request.DayOfWeek}: {result.Message}");
                }
            }

            if (allOrNothing && transaction != null)
                await transaction.CommitAsync(cancellationToken);

            return ApiResponse.Success(new
            {
                Total = results.Count,
                Succeeded = successCount,
                Failed = failureCount,
                Results = results
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error bulk upserting sport resource availabilities");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> GetSportResourceBlackoutsAsync(Guid resourceId, DateTime? fromUtc = null, DateTime? toUtc = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var resource = await sportResourceRepository.GetByIdAsync(resourceId);
            if (resource == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            var blackouts = await sportResourceBlackoutRepository.GetByResourceAsync(resourceId, fromUtc, toUtc, cancellationToken);
            var dto = blackouts.Select(BookingConfigConverter.ToDto);
            return ApiResponse.Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting sport resource blackouts for resource {ResourceId}", resourceId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> CreateSportResourceBlackoutAsync(SportResourceBlackoutCreateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var resource = await sportResourceRepository.GetByIdAsync(request.ResourceId);
            if (resource == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            var startUtc = NormalizeUtc(request.StartUtc);
            var endUtc = NormalizeUtc(request.EndUtc);

            if (endUtc <= startUtc)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "EndUtc must be greater than StartUtc.");

            var hasOverlap = await sportResourceBlackoutRepository.HasOverlapAsync(
                request.ResourceId,
                startUtc,
                endUtc,
                cancellationToken);

            if (hasOverlap)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Blackout overlaps an existing blackout.");

            var blackout = new SportResourceBlackout
            {
                FkSportResourceId = request.ResourceId,
                StartUtc = startUtc,
                EndUtc = endUtc,
                Reason = request.Reason?.Trim(),
                Active = true,
                Deleted = false
            };

            var created = await sportResourceBlackoutRepository.AddAsync(blackout);
            return ApiResponse.Success(BookingConfigConverter.ToDto(created));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating sport resource blackout for resource {ResourceId}", request.ResourceId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> DeleteSportResourceBlackoutAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var deleted = await sportResourceBlackoutRepository.DeleteAsync(id);
            return deleted
                ? ApiResponse.Success(true)
                : ApiResponse.Error(ErrorHttp.DbDeleteError);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting sport resource blackout {BlackoutId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> GetSportResourcePricingRulesAsync(Guid resourceId, CancellationToken cancellationToken = default)
    {
        try
        {
            var resource = await sportResourceRepository.GetByIdAsync(resourceId);
            if (resource == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            var rules = await sportResourcePricingRuleRepository.GetByResourceAsync(resourceId, cancellationToken);
            var dto = rules.Select(BookingConfigConverter.ToDto);
            return ApiResponse.Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting sport resource pricing rules for resource {ResourceId}", resourceId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> CreateSportResourcePricingRuleAsync(
        SportResourcePricingRuleCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var resource = await sportResourceRepository.GetByIdAsync(request.ResourceId);
            if (resource == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            var validation = ValidatePricingRule(request.PriceAmount, request.StartTime, request.EndTime, request.MinDurationMinutes);
            if (validation != null)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, validation);

            var conflict = await ValidatePricingRuleConflictsAsync(
                request.ResourceId,
                request.DayOfWeek,
                request.StartTime,
                request.EndTime,
                request.MinDurationMinutes,
                null,
                cancellationToken);
            if (conflict != null)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, conflict);

            var rule = new SportResourcePricingRule
            {
                FkSportResourceId = request.ResourceId,
                RuleType = request.RuleType,
                PriceAmount = request.PriceAmount,
                Currency = NormalizeCurrency(request.Currency),
                DayOfWeek = request.DayOfWeek,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                MinDurationMinutes = request.MinDurationMinutes,
                Active = true,
                Deleted = false
            };

            var created = await sportResourcePricingRuleRepository.AddAsync(rule);
            return ApiResponse.Success(BookingConfigConverter.ToDto(created));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating sport resource pricing rule for resource {ResourceId}", request.ResourceId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> UpdateSportResourcePricingRuleAsync(
        SportResourcePricingRuleUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var existing = await sportResourcePricingRuleRepository.GetByIdAsync(request.Id);
            if (existing == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (existing.FkSportResourceId != request.ResourceId)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Resource mismatch for pricing rule update.");

            var validation = ValidatePricingRule(request.PriceAmount, request.StartTime, request.EndTime, request.MinDurationMinutes);
            if (validation != null)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, validation);

            var conflict = await ValidatePricingRuleConflictsAsync(
                request.ResourceId,
                request.DayOfWeek,
                request.StartTime,
                request.EndTime,
                request.MinDurationMinutes,
                request.Id,
                cancellationToken);
            if (conflict != null)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, conflict);

            existing.RuleType = request.RuleType;
            existing.PriceAmount = request.PriceAmount;
            existing.Currency = NormalizeCurrency(request.Currency);
            existing.DayOfWeek = request.DayOfWeek;
            existing.StartTime = request.StartTime;
            existing.EndTime = request.EndTime;
            existing.MinDurationMinutes = request.MinDurationMinutes;

            var updated = await sportResourcePricingRuleRepository.UpdateAsync(existing);
            return ApiResponse.Success(BookingConfigConverter.ToDto(updated));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating sport resource pricing rule {RuleId}", request.Id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> DeleteSportResourcePricingRuleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var deleted = await sportResourcePricingRuleRepository.DeleteAsync(id);
            return deleted
                ? ApiResponse.Success(true)
                : ApiResponse.Error(ErrorHttp.DbDeleteError);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting sport resource pricing rule {RuleId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> UpsertSportResourcePricingRulesAsync(
        List<SportResourcePricingRuleUpsertRequest> requests,
        bool allOrNothing = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (requests == null || requests.Count == 0)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Request list is empty.");

            await using IDbContextTransaction? transaction = allOrNothing
                ? await sportResourcePricingRuleRepository.BeginTransactionAsync()
                : null;

            var results = new List<object>();
            var successCount = 0;
            var failureCount = 0;

            foreach (var request in requests)
            {
                var result = await UpsertSportResourcePricingRuleAsync(request, cancellationToken);

                if (result is ApiResultSuccess success)
                {
                    successCount++;
                    results.Add(new
                    {
                        request.ResourceId,
                        request.Id,
                        Success = true,
                        Message = success.Message,
                        Data = success.Data
                    });
                    continue;
                }

                failureCount++;
                results.Add(new
                {
                    request.ResourceId,
                    request.Id,
                    Success = false,
                    Message = result.Message
                });

                if (allOrNothing)
                {
                    if (transaction != null)
                        await transaction.RollbackAsync(cancellationToken);

                    return ApiResponse.ErrorWithMessage(
                        ErrorHttp.BadRequest,
                        $"Bulk upsert failed for resource {request.ResourceId}: {result.Message}");
                }
            }

            if (allOrNothing && transaction != null)
                await transaction.CommitAsync(cancellationToken);

            return ApiResponse.Success(new
            {
                Total = results.Count,
                Succeeded = successCount,
                Failed = failureCount,
                Results = results
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error bulk upserting sport resource pricing rules");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    private static string NormalizeCode(string code) => code.Trim().ToUpperInvariant();

    private static string NormalizeName(string name) => name.Trim();

    private static string NormalizeCurrency(string? currency)
    {
        return string.IsNullOrWhiteSpace(currency)
            ? "USD"
            : currency.Trim().ToUpperInvariant();
    }

    private static string? ValidateDuration(int? minMinutes, int? maxMinutes, int? bufferMinutes)
    {
        if (minMinutes.HasValue && minMinutes.Value <= 0)
            return "Min duration must be greater than zero.";

        if (maxMinutes.HasValue && maxMinutes.Value <= 0)
            return "Max duration must be greater than zero.";

        if (minMinutes.HasValue && maxMinutes.HasValue && minMinutes.Value > maxMinutes.Value)
            return "Min duration must be less than or equal to max duration.";

        if (bufferMinutes.HasValue && bufferMinutes.Value < 0)
            return "Buffer minutes cannot be negative.";

        return null;
    }

    private static bool IsTimeInDay(TimeSpan value)
    {
        return value >= TimeSpan.Zero && value < TimeSpan.FromDays(1);
    }

    private static string? ValidatePricingRule(
        decimal priceAmount,
        TimeSpan? startTime,
        TimeSpan? endTime,
        int? minDurationMinutes)
    {
        if (priceAmount <= 0)
            return "Price amount must be greater than zero.";

        if (minDurationMinutes.HasValue && minDurationMinutes.Value <= 0)
            return "Min duration must be greater than zero.";

        var hasStart = startTime.HasValue;
        var hasEnd = endTime.HasValue;
        if (hasStart != hasEnd)
            return "StartTime and EndTime must be provided together.";

        if (hasStart && hasEnd)
        {
            if (!IsTimeInDay(startTime!.Value) || !IsTimeInDay(endTime!.Value))
                return "StartTime and EndTime must be within a single day.";

            if (startTime >= endTime)
                return "StartTime must be before EndTime.";
        }

        return null;
    }

    private async Task<string?> ValidatePricingRuleConflictsAsync(
        Guid resourceId,
        DayOfWeek? dayOfWeek,
        TimeSpan? startTime,
        TimeSpan? endTime,
        int? minDurationMinutes,
        Guid? excludeId,
        CancellationToken cancellationToken)
    {
        var candidateStart = startTime ?? TimeSpan.Zero;
        var candidateEnd = endTime ?? TimeSpan.FromDays(1);
        var candidateSpecificity = GetRuleSpecificity(dayOfWeek, startTime, endTime, minDurationMinutes);

        var query = sportResourcePricingRuleRepository.Query()
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .Where(r =>
                r.FkSportResourceId == resourceId &&
                r.Active &&
                !r.Deleted);

        if (excludeId.HasValue)
            query = query.Where(r => r.Id != excludeId.Value);

        var rules = await query.ToListAsync(cancellationToken);
        foreach (var rule in rules)
        {
            if (!DaysOverlap(rule.DayOfWeek, dayOfWeek))
                continue;

            var ruleStart = rule.StartTime ?? TimeSpan.Zero;
            var ruleEnd = rule.EndTime ?? TimeSpan.FromDays(1);
            if (!TimesOverlap(ruleStart, ruleEnd, candidateStart, candidateEnd))
                continue;

            var existingSpecificity = GetRuleSpecificity(rule.DayOfWeek, rule.StartTime, rule.EndTime, rule.MinDurationMinutes);
            if (existingSpecificity == candidateSpecificity)
                return "Pricing rule overlaps an existing rule with the same scope. Adjust the day or time range.";
        }

        return null;
    }

    private static bool DaysOverlap(DayOfWeek? existingDay, DayOfWeek? candidateDay)
    {
        return !existingDay.HasValue || !candidateDay.HasValue || existingDay.Value == candidateDay.Value;
    }

    private static bool TimesOverlap(TimeSpan existingStart, TimeSpan existingEnd, TimeSpan candidateStart, TimeSpan candidateEnd)
    {
        return existingStart < candidateEnd && existingEnd > candidateStart;
    }

    private static int GetRuleSpecificity(
        DayOfWeek? dayOfWeek,
        TimeSpan? startTime,
        TimeSpan? endTime,
        int? minDurationMinutes)
    {
        var score = 0;
        if (dayOfWeek.HasValue) score += 2;
        if (startTime.HasValue || endTime.HasValue) score += 1;
        if (minDurationMinutes.HasValue) score += 1;
        return score;
    }

    private async Task<ApiResult> UpsertSportResourcePricingRuleAsync(
        SportResourcePricingRuleUpsertRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Id.HasValue && request.Id.Value != Guid.Empty)
        {
            var update = new SportResourcePricingRuleUpdateRequest
            {
                Id = request.Id.Value,
                ResourceId = request.ResourceId,
                RuleType = request.RuleType,
                PriceAmount = request.PriceAmount,
                Currency = request.Currency,
                DayOfWeek = request.DayOfWeek,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                MinDurationMinutes = request.MinDurationMinutes
            };

            return await UpdateSportResourcePricingRuleAsync(update, cancellationToken);
        }

        var create = new SportResourcePricingRuleCreateRequest
        {
            ResourceId = request.ResourceId,
            RuleType = request.RuleType,
            PriceAmount = request.PriceAmount,
            Currency = request.Currency,
            DayOfWeek = request.DayOfWeek,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            MinDurationMinutes = request.MinDurationMinutes
        };

        return await CreateSportResourcePricingRuleAsync(create, cancellationToken);
    }

    private static DateTime NormalizeUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
}
