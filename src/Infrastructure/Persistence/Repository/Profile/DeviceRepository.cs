using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.Interfaces.Repository.Profile;
using Bookazone.Domain.Entities.Profile;
using Bookazone.Infrastructure.Persistence.DbContext;


namespace Bookazone.Infrastructure.Persistence.Repository.Profile;

public class DeviceRepository(BookazoneDbContext BookazoneDbContext) : IDeviceRepository
{
    
    public async Task<Device?> GetByIdentifierAsync(string identifier, Guid userId)
    {
        return await BookazoneDbContext.Devices!
            .Include(d => d.FkUser)
            .FirstOrDefaultAsync(d => 
                d.Identifier == identifier && 
                d.FkUser != null && 
                d.FkUser.Id == userId &&
                d.Active && !d.Deleted);
    }

    public async Task<Device?> CreateAsync(Device entity)
    {
        entity.Active = true;
        entity.Deleted = false;
        entity.DateCreated = DateTime.UtcNow;
        await BookazoneDbContext.Devices!.AddAsync(entity);
        await BookazoneDbContext.SaveChangesAsync();
        return entity;
    }

    
    public List<Device>? All(Users? user)
    {
        return user == null ? null : (BookazoneDbContext.Devices ?? throw new Except(ErrorHttp.DbQueryRunFailed)).Where(a => a.FkUser == user && a.Deleted == false).ToListAsync().Result;
    }

    public List<Device> AllActive()
    {
        return (BookazoneDbContext.Devices ?? throw new Except(ErrorHttp.DbQueryRunFailed)).Where(
            a => a.Verified == true && a.NotificationToken != null && a.FkUser != null && a.FkUser.Active == true &&
                 a.FkUser != null && a.FkUser.Active == true &&
                 a.Active == true && a.Deleted == false
            ).ToListAsync().Result;
    }

    public List<Device> GetUserDevicesWithTokens(Users userId)
    {
        return BookazoneDbContext.Devices!
            .Where(d => d.FkUser != null && d.FkUser.Id == userId.Id &&
                        d.Deleted != true &&
                        d.Deleted != true && 
                        d.Active == true && 
                        !string.IsNullOrEmpty(d.NotificationToken))
            .ToList();
    }

    public Device? Find(Guid? id)
    {
        if (BookazoneDbContext.Devices == null) throw new Except(ErrorHttp.DbQueryRunFailed);
        return BookazoneDbContext.Devices.FindAsync(id).Result;
    }

    public Device? FindByIdentifier(string? identifier)
    {
        if (BookazoneDbContext.Devices == null) throw new Except(ErrorHttp.DbQueryRunFailed);
        return identifier != null ? BookazoneDbContext.Devices.FirstOrDefault(a => a.Identifier == identifier && a.Active == true) : null;
    }

    public Device? FindByIdAndUser(Guid? id, Users? user)
    {
        if (BookazoneDbContext.Devices == null) throw new Except(ErrorHttp.DbQueryRunFailed);
        return id != null && user != null ? BookazoneDbContext.Devices.FirstOrDefault(a => a.Id == id && a.FkUser == user && a.Active == true) : null;
    }

    public List<Device>? ListByIdentifierAndUser(string? identifier, Users? user)
    {
        if (BookazoneDbContext.Devices == null) throw new Except(ErrorHttp.DbQueryRunFailed);
        return identifier != null && user != null ? BookazoneDbContext.Devices.Where(a => a.Identifier == identifier && a.FkUser == user && a.Active == true)
            .OrderByDescending(a => a.Id)
            .ToListAsync().Result : null;
    }

    public List<Device>? ListByUser(Users? user)
    {
        if (BookazoneDbContext.Devices == null) throw new Except(ErrorHttp.DbQueryRunFailed);
        return user != null ? BookazoneDbContext.Devices.Where(a => a.FkUser == user && a.Active == true).ToListAsync().Result : null;
    }
    public Device? FindByIdentifierAndUser(string? identifier, Users? user)
    {
        if (BookazoneDbContext.Devices == null) throw new Except(ErrorHttp.DbQueryRunFailed);
        return identifier != null && user != null ? BookazoneDbContext.Devices.Where(a => a.Identifier == identifier && a.FkUser == user && a.Active == true)
            .OrderByDescending(a => a.Id).FirstOrDefault() : null;
    }

    public Device? FindByToken(string? token)
    {
        if (BookazoneDbContext.Devices == null) throw new Except(ErrorHttp.DbQueryRunFailed);
        return token != null ? BookazoneDbContext.Devices.FirstOrDefault(a => a.CurrentSessionToken == token && a.Active == true) : null;
    }

    public Device? Create(Device entity)
    {
        entity.Active = true;
        entity.Deleted = false;
        entity.DateCreated = DateTime.UtcNow;
        if (BookazoneDbContext.Devices == null) throw new Except(ErrorHttp.DbCreateError);
        BookazoneDbContext.Devices.Add(entity);
        var result = BookazoneDbContext.SaveChanges(); // Changed from SaveChangesAsync().Result
        return result > 0 ? entity : null;
    }


    public Device? Update(Device entity)
    {
        entity.DateUpdated = DateTime.UtcNow;
        if (BookazoneDbContext.Devices == null) throw new Except(ErrorHttp.DbCreateError);
        BookazoneDbContext.Devices.Update(entity);
        var result = BookazoneDbContext.SaveChanges();
        return result > 0 ? entity : null;
    }
    
    public bool Delete(Guid? id)
    {
        var entity = Find(id);
        if (id == null || entity == null) throw new Except(ErrorHttp.NotFound);
        entity.Active = false;
        entity.Deleted = true;
        entity.DateDeleted = DateTime.UtcNow;
        if (BookazoneDbContext.Devices == null) throw new Except(ErrorHttp.DbCreateError);
        BookazoneDbContext.Devices.Update(entity);
        var result = BookazoneDbContext.SaveChangesAsync().Result; return result > 0;
    }

    public bool Status(Guid? id, bool active)
    {
        var entity = Find(id);
        if (id == null || entity == null) throw new Except(ErrorHttp.NotFound);
        entity.Active = active;
        entity.DateUpdated = DateTime.UtcNow;
        if (BookazoneDbContext.Devices == null) throw new Except(ErrorHttp.DbCreateError);
        BookazoneDbContext.Devices.Update(entity);
        var result = BookazoneDbContext.SaveChangesAsync().Result; return result > 0;
    }
}