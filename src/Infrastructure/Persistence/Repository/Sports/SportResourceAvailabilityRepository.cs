using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Interfaces.Repository.Sports;
using Bookazone.Domain.Entities.Sports;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Persistence.Repository;

namespace Bookazone.Infrastructure.Persistence.Repository.Sports;

public class SportResourceAvailabilityRepository(
    BookazoneDbContext context,
    ILogger<SportResourceAvailabilityRepository> logger)
    : BaseRepository<SportResourceAvailability>(context, logger), ISportResourceAvailabilityRepository
{
    public async Task<List<SportResourceAvailability>> GetByResourceAsync(Guid resourceId, CancellationToken cancellationToken = default)
    {
        return await Context.SportResourceAvailabilities
            .AsNoTracking()
            .Where(a =>
                a.FkSportResourceId == resourceId &&
                a.Active &&
                !a.Deleted)
            .OrderBy(a => a.DayOfWeek)
            .ToListAsync(cancellationToken);
    }

    public async Task<SportResourceAvailability?> GetByResourceAndDayAsync(Guid resourceId, DayOfWeek dayOfWeek, CancellationToken cancellationToken = default)
    {
        return await Context.SportResourceAvailabilities
            .AsNoTracking()
            .FirstOrDefaultAsync(a =>
                a.FkSportResourceId == resourceId &&
                a.DayOfWeek == dayOfWeek &&
                a.Active &&
                !a.Deleted,
                cancellationToken);
    }

    public async Task<SportResourceAvailability> CreateOrUpdateAsync(SportResourceAvailability availability, CancellationToken cancellationToken = default)
    {
        var existing = await Context.SportResourceAvailabilities
            .FirstOrDefaultAsync(a =>
                a.FkSportResourceId == availability.FkSportResourceId &&
                a.DayOfWeek == availability.DayOfWeek &&
                !a.Deleted,
                cancellationToken);

        if (existing == null)
        {
            availability.Active = true;
            availability.Deleted = false;
            await Context.SportResourceAvailabilities.AddAsync(availability, cancellationToken);
            await Context.SaveChangesAsync(cancellationToken);
            return availability;
        }

        existing.OpenTime = availability.OpenTime;
        existing.CloseTime = availability.CloseTime;
        existing.IsClosed = availability.IsClosed;
        existing.Active = availability.Active;
        existing.Deleted = availability.Deleted;
        Context.SportResourceAvailabilities.Update(existing);
        await Context.SaveChangesAsync(cancellationToken);
        return existing;
    }
}
