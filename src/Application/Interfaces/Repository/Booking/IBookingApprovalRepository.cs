using Bookazone.Application.Interfaces.Repository;
using Bookazone.Domain.Entities.Booking;

namespace Bookazone.Application.Interfaces.Repository.Booking;

public interface IBookingApprovalRepository : IGenericRepository<BookingApproval>
{
    Task<BookingApproval?> GetByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default);
}
