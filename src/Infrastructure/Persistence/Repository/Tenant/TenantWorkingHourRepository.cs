using Bookazone.Application.Interfaces.Repository.Tenant;
using Bookazone.Domain.Entities.Tenant;
using Bookazone.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Bookazone.Infrastructure.Persistence.Repository.Tenant;

public class TenantWorkingHourRepository(
    BookazoneDbContext context,
    ILogger<TenantWorkingHourRepository> logger)
    : BaseRepository<TenantWorkingHour>(context, logger), ITenantWorkingHourRepository
{
    public async Task<List<TenantWorkingHour>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await Context.TenantWorkingHours
            .AsNoTracking()
            .Where(w =>
                w.FkTenantId == tenantId &&
                w.Active &&
                !w.Deleted)
            .OrderBy(w => w.DayOfWeek)
            .ToListAsync(cancellationToken);
    }

    public async Task<TenantWorkingHour?> GetByTenantAndDayAsync(Guid tenantId, DayOfWeek dayOfWeek, CancellationToken cancellationToken = default)
    {
        return await Context.TenantWorkingHours
            .AsNoTracking()
            .FirstOrDefaultAsync(w =>
                w.FkTenantId == tenantId &&
                w.DayOfWeek == dayOfWeek &&
                w.Active &&
                !w.Deleted,
                cancellationToken);
    }

    public async Task<TenantWorkingHour> CreateOrUpdateAsync(TenantWorkingHour workingHour, CancellationToken cancellationToken = default)
    {
        var existing = await Context.TenantWorkingHours
            .FirstOrDefaultAsync(w =>
                w.FkTenantId == workingHour.FkTenantId &&
                w.DayOfWeek == workingHour.DayOfWeek &&
                !w.Deleted,
                cancellationToken);

        if (existing == null)
        {
            workingHour.Active = true;
            workingHour.Deleted = false;
            await Context.TenantWorkingHours.AddAsync(workingHour, cancellationToken);
            await Context.SaveChangesAsync(cancellationToken);
            return workingHour;
        }

        existing.OpenTime = workingHour.OpenTime;
        existing.CloseTime = workingHour.CloseTime;
        existing.IsClosed = workingHour.IsClosed;
        existing.SlotDurationMinutes = workingHour.SlotDurationMinutes;
        existing.Active = workingHour.Active;
        existing.Deleted = workingHour.Deleted;
        Context.TenantWorkingHours.Update(existing);
        await Context.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<bool> IsValidBookingSlotAsync(Guid tenantId, DateTime startLocal, DateTime endLocal, CancellationToken cancellationToken = default)
    {
        if (endLocal <= startLocal) return false;
        if (startLocal.Date != endLocal.Date) return false;

        var workingHour = await Context.TenantWorkingHours
            .AsNoTracking()
            .FirstOrDefaultAsync(w =>
                w.FkTenantId == tenantId &&
                w.DayOfWeek == startLocal.DayOfWeek &&
                w.Active &&
                !w.Deleted,
                cancellationToken);

        if (workingHour == null || workingHour.IsClosed) return false;
        if (workingHour.OpenTime == null || workingHour.CloseTime == null) return false;

        var startTime = startLocal.TimeOfDay;
        var endTime = endLocal.TimeOfDay;

        if (startTime < workingHour.OpenTime || endTime > workingHour.CloseTime) return false;

        var slotMinutes = workingHour.SlotDurationMinutes;
        if (slotMinutes <= 0) return false;

        var durationMinutes = (int)(endTime - startTime).TotalMinutes;
        if (durationMinutes <= 0 || durationMinutes % slotMinutes != 0) return false;

        var offsetMinutes = (int)(startTime - workingHour.OpenTime.Value).TotalMinutes;
        if (offsetMinutes < 0 || offsetMinutes % slotMinutes != 0) return false;

        return true;
    }
}
