using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Interfaces.Repository.Booking;
using Bookazone.Domain.Entities.Booking;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Persistence.Repository;

namespace Bookazone.Infrastructure.Persistence.Repository.Booking;

public class BookingApprovalRepository(
    BookazoneDbContext context,
    ILogger<BookingApprovalRepository> logger)
    : BaseRepository<BookingApproval>(context, logger), IBookingApprovalRepository
{
    public async Task<BookingApproval?> GetByBookingIdAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        return await Context.BookingApprovals
            .AsNoTracking()
            .FirstOrDefaultAsync(ba =>
                ba.FkBookingId == bookingId &&
                ba.Active &&
                !ba.Deleted,
                cancellationToken);
    }
}
