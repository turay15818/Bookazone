namespace Bookazone.Application.DTOs.Booking;

public class BookingValidationResult
{
    public bool IsValid { get; private set; }
    public string Code { get; private set; } = BookingValidationErrorCodes.Ok;
    public string Message { get; private set; } = "Valid";

    public static BookingValidationResult Success()
        => new() { IsValid = true, Code = BookingValidationErrorCodes.Ok, Message = "Valid" };

    public static BookingValidationResult Fail(string code, string message)
        => new() { IsValid = false, Code = code, Message = message };
}

public static class BookingValidationErrorCodes
{
    public const string Ok = "OK";
    public const string InvalidTimeRange = "InvalidTimeRange";
    public const string ResourceNotFound = "ResourceNotFound";
    public const string ResourceInactive = "ResourceInactive";
    public const string ResourceTenantMismatch = "ResourceTenantMismatch";
    public const string InvalidAttendeeCount = "InvalidAttendeeCount";
    public const string CapacityExceeded = "CapacityExceeded";
    public const string CrossDayBooking = "CrossDayBooking";
    public const string TenantHoursViolation = "TenantHoursViolation";
    public const string AvailabilityNotConfigured = "AvailabilityNotConfigured";
    public const string AvailabilityClosed = "AvailabilityClosed";
    public const string AvailabilityOutOfRange = "AvailabilityOutOfRange";
    public const string BlackoutConflict = "BlackoutConflict";
    public const string BookingOverlap = "BookingOverlap";
}
