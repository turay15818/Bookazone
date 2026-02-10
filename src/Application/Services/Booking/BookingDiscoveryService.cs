using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Booking;
using Bookazone.Application.DTOs.Converter;
using Bookazone.Application.DTOs.Response;
using Bookazone.Application.Interfaces.Repository.Booking;
using Bookazone.Application.Interfaces.Repository.Sports;
using Bookazone.Application.Interfaces.Repository.Subscription;
using Bookazone.Application.Interfaces.Repository.Tenant;
using Bookazone.Application.Interfaces.Services.Booking;
using Bookazone.Domain.Enums;

namespace Bookazone.Application.Services.Booking;

public class BookingDiscoveryService(
    ITenantSportTypeRepository tenantSportTypeRepository,
    ITenantBookingCategoryRepository tenantBookingCategoryRepository,
    ISportResourceRepository sportResourceRepository,
    ISportResourceAvailabilityRepository sportResourceAvailabilityRepository,
    ISportResourceBlackoutRepository sportResourceBlackoutRepository,
    ITenantWorkingHourRepository tenantWorkingHourRepository,
    ITenantSettingsRepository tenantSettingsRepository,
    IBookingRepository bookingRepository,
    ILogger<BookingDiscoveryService> logger)
    : IBookingDiscoveryService
{
    public async Task<ApiResult> GetSportTypesAsync(Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = tenantSportTypeRepository.Query()
                .AsNoTracking()
                .Where(t => t.Active && !t.Deleted);

            if (tenantId.HasValue)
                query = query.Where(t => t.FkTenantId == tenantId.Value);

            var types = await query
                .OrderBy(t => t.Name)
                .ToListAsync(cancellationToken);

            var dto = types.Select(BookingDiscoveryConverter.ToPublicDto);
            return ApiResponse.Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting sport types");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> GetTenantsAsync(
        BookingCategoryType? category = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = tenantBookingCategoryRepository.Query()
                .AsNoTracking()
                .Where(tbc =>
                    tbc.Active &&
                    !tbc.Deleted &&
                    tbc.IsEnabled &&
                    tbc.FkTenant.Active &&
                    !tbc.FkTenant.Deleted &&
                    tbc.FkBookingCategory.Active &&
                    !tbc.FkBookingCategory.Deleted);

            if (category.HasValue)
            {
                var code = BookingCategoryTypeMapper.ToCode(category.Value);
                query = query.Where(tbc => tbc.FkBookingCategory.Code == code);
            }

            var tenants = await query
                .Select(tbc => new
                {
                    tbc.FkTenant.Id,
                    tbc.FkTenant.Name,
                    tbc.FkTenant.Description,
                    tbc.FkTenant.Address,
                    tbc.FkTenant.City,
                    tbc.FkTenant.State,
                    tbc.FkTenant.Country,
                    tbc.FkTenant.Logo,
                    tbc.FkTenant.IsVerified
                })
                .Distinct()
                .OrderBy(t => t.Name)
                .ToListAsync(cancellationToken);

            var dto = tenants.Select(t => new VwTenantPublic
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description,
                Address = t.Address,
                City = t.City,
                State = t.State,
                Country = t.Country,
                Logo = t.Logo,
                IsVerified = t.IsVerified
            });
            return ApiResponse.Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting tenants for category {Category}", category);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> SearchSportResourcesAsync(
        SportResourceSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            request ??= new SportResourceSearchRequest();

            var query = sportResourceRepository.Query()
                .AsNoTracking()
                .Include(r => r.FkTenant)
                .Include(r => r.FkTenantSportType)
                .Where(r => r.Active && !r.Deleted && r.FkTenant.Active && !r.FkTenant.Deleted);

            if (request.TenantId.HasValue)
                query = query.Where(r => r.FkTenantId == request.TenantId.Value);

            if (request.SportTypeId.HasValue)
                query = query.Where(r => r.FkTenantSportTypeId == request.SportTypeId.Value);

            if (request.MinCapacity.HasValue)
                query = query.Where(r => r.Capacity.HasValue && r.Capacity.Value >= request.MinCapacity.Value);

            if (!string.IsNullOrWhiteSpace(request.City))
            {
                var city = request.City.Trim().ToLowerInvariant();
                query = query.Where(r => r.FkTenant.City != null && r.FkTenant.City.ToLower().Contains(city));
            }

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var term = request.Search.Trim().ToLowerInvariant();
                query = query.Where(r =>
                    r.Name.ToLower().Contains(term) ||
                    (r.Description != null && r.Description.ToLower().Contains(term)) ||
                    r.FkTenant.Name.ToLower().Contains(term));
            }

            var resources = await query
                .OrderBy(r => r.FkTenant.Name)
                .ThenBy(r => r.Name)
                .ToListAsync(cancellationToken);

            var resourceIds = resources.Select(r => r.Id).ToList();
            var availabilityLookup = await BuildAvailabilityLookupAsync(resourceIds, cancellationToken);

            var dto = resources.Select(r =>
                BookingDiscoveryConverter.ToPublicDto(
                    r,
                    availabilityLookup.GetValueOrDefault(r.Id)));
            return ApiResponse.Success(dto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching sport resources");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> GetSportResourceAsync(Guid resourceId, CancellationToken cancellationToken = default)
    {
        try
        {
            var resource = await sportResourceRepository.Query()
                .AsNoTracking()
                .Include(r => r.FkTenant)
                .Include(r => r.FkTenantSportType)
                .FirstOrDefaultAsync(r => r.Id == resourceId && r.Active && !r.Deleted, cancellationToken);

            if (resource == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            var availabilityLookup = await BuildAvailabilityLookupAsync(
                new List<Guid> { resourceId },
                cancellationToken);

            return ApiResponse.Success(BookingDiscoveryConverter.ToPublicDto(
                resource,
                availabilityLookup.GetValueOrDefault(resourceId)));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting sport resource {ResourceId}", resourceId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> GetSportResourceAvailabilityAsync(Guid resourceId, CancellationToken cancellationToken = default)
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

    public async Task<ApiResult> GetSportResourceSlotsAsync(
        Guid resourceId,
        DateTime dateLocal,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var resource = await sportResourceRepository.GetByIdAsync(resourceId);
            if (resource == null || !resource.Active || resource.Deleted)
                return ApiResponse.Error(ErrorHttp.NotFound);

            var localDate = dateLocal.Date;
            var dayOfWeek = localDate.DayOfWeek;
            var tenantId = resource.FkTenantId;

            var tenantHours = await tenantWorkingHourRepository.GetByTenantAndDayAsync(
                tenantId,
                dayOfWeek,
                cancellationToken);
            var availability = await sportResourceAvailabilityRepository.GetByResourceAndDayAsync(
                resourceId,
                dayOfWeek,
                cancellationToken);

            var summary = new VwSportResourceSlotSummary
            {
                ResourceId = resourceId,
                DateLocal = localDate,
                DayOfWeek = dayOfWeek
            };

            if (tenantHours == null || tenantHours.IsClosed || tenantHours.OpenTime == null || tenantHours.CloseTime == null)
            {
                summary.IsClosed = true;
                summary.StatusMessage = "Tenant working hours are closed or not configured for this day.";
                return ApiResponse.Success(summary);
            }

            if (availability == null || availability.IsClosed || availability.OpenTime == null || availability.CloseTime == null)
            {
                summary.IsClosed = true;
                summary.StatusMessage = "Resource availability is closed or not configured for this day.";
                return ApiResponse.Success(summary);
            }

            var openTime = tenantHours.OpenTime.Value > availability.OpenTime.Value
                ? tenantHours.OpenTime.Value
                : availability.OpenTime.Value;
            var closeTime = tenantHours.CloseTime.Value < availability.CloseTime.Value
                ? tenantHours.CloseTime.Value
                : availability.CloseTime.Value;

            if (tenantHours.SlotDurationMinutes <= 0 || openTime >= closeTime)
            {
                summary.IsClosed = true;
                summary.StatusMessage = "No available slots for this day.";
                summary.SlotDurationMinutes = tenantHours.SlotDurationMinutes;
                return ApiResponse.Success(summary);
            }

            summary.OpenTime = openTime;
            summary.CloseTime = closeTime;
            summary.SlotDurationMinutes = tenantHours.SlotDurationMinutes;
            summary.IsClosed = false;

            var timeZone = await ResolveTenantTimeZoneAsync(tenantId);
            var dayStartLocal = DateTime.SpecifyKind(localDate, DateTimeKind.Unspecified);
            var dayEndLocal = DateTime.SpecifyKind(localDate.AddDays(1), DateTimeKind.Unspecified);
            var dayStartUtc = ConvertLocalToUtc(dayStartLocal, timeZone);
            var dayEndUtc = ConvertLocalToUtc(dayEndLocal, timeZone);

            var bookings = await bookingRepository.GetByResourceAsync(
                resourceId,
                dayStartUtc,
                dayEndUtc,
                cancellationToken);
            var blackouts = await sportResourceBlackoutRepository.GetByResourceAsync(
                resourceId,
                dayStartUtc,
                dayEndUtc,
                cancellationToken);

            var activeBookings = bookings
                .Where(b => b.Status != BookingStatus.Cancelled && b.Status != BookingStatus.Declined)
                .ToList();

            var slotDuration = TimeSpan.FromMinutes(tenantHours.SlotDurationMinutes);
            var slotStartLocal = dayStartLocal.Add(openTime);
            var lastSlotEndLocal = dayStartLocal.Add(closeTime);

            while (slotStartLocal + slotDuration <= lastSlotEndLocal)
            {
                var slotEndLocal = slotStartLocal + slotDuration;
                var slotStartUtc = ConvertLocalToUtc(slotStartLocal, timeZone);
                var slotEndUtc = ConvertLocalToUtc(slotEndLocal, timeZone);

                var isBooked = activeBookings.Any(b => b.StartUtc < slotEndUtc && b.EndUtc > slotStartUtc);
                var isBlackout = blackouts.Any(b => b.StartUtc < slotEndUtc && b.EndUtc > slotStartUtc);
                var isAvailable = !isBooked && !isBlackout;

                summary.Slots.Add(new VwSportResourceSlot
                {
                    StartLocal = slotStartLocal,
                    EndLocal = slotEndLocal,
                    StartUtc = slotStartUtc,
                    EndUtc = slotEndUtc,
                    IsAvailable = isAvailable,
                    UnavailableReason = isAvailable
                        ? null
                        : isBooked
                            ? "Booked"
                            : "Blackout"
                });

                slotStartLocal = slotEndLocal;
            }

            return ApiResponse.Success(summary);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting sport resource slots {ResourceId}", resourceId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    private async Task<Dictionary<Guid, List<VwSportResourceAvailability>>> BuildAvailabilityLookupAsync(
        List<Guid> resourceIds,
        CancellationToken cancellationToken)
    {
        if (resourceIds.Count == 0)
            return new Dictionary<Guid, List<VwSportResourceAvailability>>();

        var availability = await sportResourceAvailabilityRepository.Query()
            .AsNoTracking()
            .Where(a =>
                resourceIds.Contains(a.FkSportResourceId) &&
                a.Active &&
                !a.Deleted)
            .OrderBy(a => a.DayOfWeek)
            .ToListAsync(cancellationToken);

        return availability
            .GroupBy(a => a.FkSportResourceId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(BookingConfigConverter.ToDto).ToList());
    }

    private async Task<TimeZoneInfo> ResolveTenantTimeZoneAsync(Guid tenantId)
    {
        var settings = await tenantSettingsRepository.GetByTenantAsync(tenantId);
        if (string.IsNullOrWhiteSpace(settings?.DefaultTimeZone))
            return TimeZoneInfo.Utc;

        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(settings.DefaultTimeZone);
        }
        catch (TimeZoneNotFoundException ex)
        {
            logger.LogWarning(ex, "Time zone not found for tenant {TenantId}: {TimeZoneId}", tenantId, settings.DefaultTimeZone);
            return TimeZoneInfo.Utc;
        }
        catch (InvalidTimeZoneException ex)
        {
            logger.LogWarning(ex, "Invalid time zone for tenant {TenantId}: {TimeZoneId}", tenantId, settings.DefaultTimeZone);
            return TimeZoneInfo.Utc;
        }
    }

    private static DateTime ConvertLocalToUtc(DateTime localTime, TimeZoneInfo timeZone)
    {
        var unspecified = DateTime.SpecifyKind(localTime, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(unspecified, timeZone);
    }
}
