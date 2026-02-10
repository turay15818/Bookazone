using Bookazone.Application.DTOs.Booking;
using Bookazone.Application.Interfaces.Repository.Booking;
using Bookazone.Application.Interfaces.Repository.Sports;
using Bookazone.Application.Interfaces.Repository.Subscription;
using Bookazone.Application.Interfaces.Repository.Tenant;
using Bookazone.Application.Interfaces.Services.Booking;
using Bookazone.Domain.Entities.Sports;

namespace Bookazone.Application.Services.Booking;

public class BookingValidationService(
    ITenantWorkingHourRepository tenantWorkingHourRepository,
    ITenantSettingsRepository tenantSettingsRepository,
    ISportResourceRepository sportResourceRepository,
    ISportResourceAvailabilityRepository sportResourceAvailabilityRepository,
    ISportResourceBlackoutRepository sportResourceBlackoutRepository,
    IBookingRepository bookingRepository,
    ILogger<BookingValidationService> logger)
    : IBookingValidationService
{
    public async Task<BookingValidationResult> ValidateBookingAsync(
        BookingValidationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.EndUtc <= request.StartUtc)
            return BookingValidationResult.Fail(
                BookingValidationErrorCodes.InvalidTimeRange,
                "End time must be greater than start time.");

        var resource = await sportResourceRepository.GetByIdAsync(request.ResourceId);
        if (resource == null)
            return BookingValidationResult.Fail(
                BookingValidationErrorCodes.ResourceNotFound,
                "Resource not found.");

        if (!resource.Active)
            return BookingValidationResult.Fail(
                BookingValidationErrorCodes.ResourceInactive,
                "Resource is inactive.");

        if (resource.FkTenantId != request.TenantId)
            return BookingValidationResult.Fail(
                BookingValidationErrorCodes.ResourceTenantMismatch,
                "Resource does not belong to the tenant.");

        if (request.ExpectedAttendees.HasValue)
        {
            if (request.ExpectedAttendees.Value <= 0)
                return BookingValidationResult.Fail(
                    BookingValidationErrorCodes.InvalidAttendeeCount,
                    "Expected attendees must be greater than zero.");

            if (resource.Capacity.HasValue && request.ExpectedAttendees.Value > resource.Capacity.Value)
                return BookingValidationResult.Fail(
                    BookingValidationErrorCodes.CapacityExceeded,
                    "Expected attendees exceed resource capacity.");
        }

        var localRange = await GetLocalRangeAsync(
            request.TenantId,
            request.StartUtc,
            request.EndUtc,
            cancellationToken);

        if (localRange.StartLocal.Date != localRange.EndLocal.Date)
            return BookingValidationResult.Fail(
                BookingValidationErrorCodes.CrossDayBooking,
                "Booking must start and end on the same local day.");

        var tenantHoursResult = await ValidateTenantHoursAsync(
            request.TenantId,
            request.StartUtc,
            request.EndUtc,
            cancellationToken);
        if (!tenantHoursResult.IsValid) return tenantHoursResult;

        var availabilityResult = await ValidateResourceAvailabilityAsync(
            request.TenantId,
            request.ResourceId,
            request.StartUtc,
            request.EndUtc,
            cancellationToken);
        if (!availabilityResult.IsValid) return availabilityResult;

        var blackoutResult = await ValidateResourceBlackoutsAsync(
            request.ResourceId,
            request.StartUtc,
            request.EndUtc,
            cancellationToken);
        if (!blackoutResult.IsValid) return blackoutResult;

        var overlapResult = await ValidateOverlapsAsync(
            request.ResourceId,
            request.StartUtc,
            request.EndUtc,
            request.BookingId,
            cancellationToken);
        if (!overlapResult.IsValid) return overlapResult;

        return BookingValidationResult.Success();
    }

    public async Task<BookingValidationResult> ValidateTenantHoursAsync(
        Guid tenantId,
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken = default)
    {
        var localRange = await GetLocalRangeAsync(tenantId, startUtc, endUtc, cancellationToken);
        if (localRange.StartLocal.Date != localRange.EndLocal.Date)
            return BookingValidationResult.Fail(
                BookingValidationErrorCodes.CrossDayBooking,
                "Booking must start and end on the same local day.");

        var valid = await tenantWorkingHourRepository.IsValidBookingSlotAsync(
            tenantId,
            localRange.StartLocal,
            localRange.EndLocal,
            cancellationToken);

        return valid
            ? BookingValidationResult.Success()
            : BookingValidationResult.Fail(
                BookingValidationErrorCodes.TenantHoursViolation,
                "Booking time is outside tenant working hours or slot rules.");
    }

    public async Task<BookingValidationResult> ValidateResourceAvailabilityAsync(
        Guid tenantId,
        Guid resourceId,
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken = default)
    {
        var localRange = await GetLocalRangeAsync(tenantId, startUtc, endUtc, cancellationToken);
        if (localRange.StartLocal.Date != localRange.EndLocal.Date)
            return BookingValidationResult.Fail(
                BookingValidationErrorCodes.CrossDayBooking,
                "Booking must start and end on the same local day.");

        var availability = await sportResourceAvailabilityRepository.GetByResourceAndDayAsync(
            resourceId,
            localRange.StartLocal.DayOfWeek,
            cancellationToken);

        if (availability == null)
            return BookingValidationResult.Fail(
                BookingValidationErrorCodes.AvailabilityNotConfigured,
                "Resource availability is not configured for this day.");

        if (availability.IsClosed)
            return BookingValidationResult.Fail(
                BookingValidationErrorCodes.AvailabilityClosed,
                "Resource is closed on this day.");

        if (availability.OpenTime == null || availability.CloseTime == null)
            return BookingValidationResult.Fail(
                BookingValidationErrorCodes.AvailabilityNotConfigured,
                "Resource availability hours are incomplete.");

        if (!IsWithinAvailability(availability, localRange.StartLocal, localRange.EndLocal))
            return BookingValidationResult.Fail(
                BookingValidationErrorCodes.AvailabilityOutOfRange,
                "Booking time is outside resource availability.");

        return BookingValidationResult.Success();
    }

    public async Task<BookingValidationResult> ValidateResourceBlackoutsAsync(
        Guid resourceId,
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken = default)
    {
        var hasOverlap = await sportResourceBlackoutRepository.HasOverlapAsync(
            resourceId,
            NormalizeUtc(startUtc),
            NormalizeUtc(endUtc),
            cancellationToken);

        return hasOverlap
            ? BookingValidationResult.Fail(
                BookingValidationErrorCodes.BlackoutConflict,
                "Booking conflicts with a blackout period.")
            : BookingValidationResult.Success();
    }

    public async Task<BookingValidationResult> ValidateOverlapsAsync(
        Guid resourceId,
        DateTime startUtc,
        DateTime endUtc,
        Guid? excludeBookingId = null,
        CancellationToken cancellationToken = default)
    {
        var hasOverlap = await bookingRepository.HasOverlapAsync(
            resourceId,
            NormalizeUtc(startUtc),
            NormalizeUtc(endUtc),
            excludeBookingId,
            cancellationToken);

        return hasOverlap
            ? BookingValidationResult.Fail(
                BookingValidationErrorCodes.BookingOverlap,
                "Booking overlaps an existing reservation.")
            : BookingValidationResult.Success();
    }

    private async Task<LocalRange> GetLocalRangeAsync(
        Guid tenantId,
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken)
    {
        var timeZone = await ResolveTenantTimeZoneAsync(tenantId);
        var normalizedStart = NormalizeUtc(startUtc);
        var normalizedEnd = NormalizeUtc(endUtc);

        return new LocalRange(
            TimeZoneInfo.ConvertTimeFromUtc(normalizedStart, timeZone),
            TimeZoneInfo.ConvertTimeFromUtc(normalizedEnd, timeZone));
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

    private static DateTime NormalizeUtc(DateTime value)
    {
        return value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }

    private static bool IsWithinAvailability(
        SportResourceAvailability availability,
        DateTime startLocal,
        DateTime endLocal)
    {
        var startTime = startLocal.TimeOfDay;
        var endTime = endLocal.TimeOfDay;

        return availability.OpenTime != null &&
               availability.CloseTime != null &&
               startTime >= availability.OpenTime &&
               endTime <= availability.CloseTime;
    }

    private sealed record LocalRange(DateTime StartLocal, DateTime EndLocal);
}
