using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;
using Bookazone.Domain.Enums;

namespace Bookazone.Domain.Entities.Booking;

public class BookingCategory : UserBaseEntity
{
    [MaxLength(100)] public string Name { get; set; } = null!;
    [MaxLength(50)] public string Code { get; set; } = null!;
    [MaxLength(500)] public string? Description { get; set; }
    public BookingMode DefaultBookingMode { get; set; } = BookingMode.Instant;
}
