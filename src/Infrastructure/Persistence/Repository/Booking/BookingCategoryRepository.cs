using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Interfaces.Repository.Booking;
using Bookazone.Domain.Entities.Booking;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Persistence.Repository;

namespace Bookazone.Infrastructure.Persistence.Repository.Booking;

public class BookingCategoryRepository(
    BookazoneDbContext context,
    ILogger<BookingCategoryRepository> logger)
    : BaseRepository<BookingCategory>(context, logger), IBookingCategoryRepository
{
    public async Task<BookingCategory?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;

        var normalized = code.Trim().ToLower();
        return await Context.BookingCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(c =>
                c.Code.ToLower() == normalized &&
                c.Active &&
                !c.Deleted,
                cancellationToken);
    }

    public async Task<List<BookingCategory>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await Context.BookingCategories
            .AsNoTracking()
            .Where(c => c.Active && !c.Deleted)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }
}
