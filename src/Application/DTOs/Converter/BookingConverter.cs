using Bookazone.Application.DTOs.Response;
using BookingEntity = Bookazone.Domain.Entities.Booking.Booking;

namespace Bookazone.Application.DTOs.Converter;

public static class BookingConverter
{
    public static VwBooking ToDto(BookingEntity booking)
    {
        return new VwBooking
        {
            Id = booking.Id,
            TenantId = booking.FkTenantId,
            ResourceId = booking.FkSportResourceId,
            CustomerId = booking.FkCustomerId,
            Status = booking.Status,
            BookingMode = booking.BookingMode,
            StartUtc = booking.StartUtc,
            EndUtc = booking.EndUtc,
            ExpectedAttendees = booking.ExpectedAttendees,
            TotalPrice = booking.TotalPrice,
            Currency = booking.Currency,
            Notes = booking.Notes
        };
    }
}
