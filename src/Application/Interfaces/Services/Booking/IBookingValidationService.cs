using Bookazone.Application.DTOs.Booking;

namespace Bookazone.Application.Interfaces.Services.Booking;

public interface IBookingValidationService
{
    Task<BookingValidationResult> ValidateBookingAsync(
        BookingValidationRequest request,
        CancellationToken cancellationToken = default);

    Task<BookingValidationResult> ValidateTenantHoursAsync(
        Guid tenantId,
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken = default);

    Task<BookingValidationResult> ValidateResourceAvailabilityAsync(
        Guid tenantId,
        Guid resourceId,
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken = default);

    Task<BookingValidationResult> ValidateResourceBlackoutsAsync(
        Guid resourceId,
        DateTime startUtc,
        DateTime endUtc,
        CancellationToken cancellationToken = default);

    Task<BookingValidationResult> ValidateOverlapsAsync(
        Guid resourceId,
        DateTime startUtc,
        DateTime endUtc,
        Guid? excludeBookingId = null,
        CancellationToken cancellationToken = default);
}
