using Bookazone.Application.Interfaces.Repository;
using Bookazone.Domain.Entities.Booking;

namespace Bookazone.Application.Interfaces.Repository.Booking;

public interface IBookingCategoryRepository : IGenericRepository<BookingCategory>
{
    Task<BookingCategory?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<List<BookingCategory>> GetActiveAsync(CancellationToken cancellationToken = default);
}
