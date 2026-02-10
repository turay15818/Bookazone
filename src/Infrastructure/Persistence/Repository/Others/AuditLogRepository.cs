using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.Interfaces.Repository.Others;
using Bookazone.Domain.Entities.Others;
using Bookazone.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Bookazone.Infrastructure.Persistence.Repository.Others;

public class AuditLogRepository(BookazoneDbContext bookazone) : IAuditLogRepository
{
    public List<AuditLog> All()
    {
        return (bookazone.AuditLogs ?? throw new Except(ErrorHttp.DbQueryRunFailed))
            .Where(a => a.Deleted == false).ToListAsync().Result;
    }

    public AuditLog? Find(int? id)
    {
        if (bookazone.AuditLogs == null) throw new Except(ErrorHttp.DbQueryRunFailed);
        return bookazone.AuditLogs.FindAsync(id).Result;
    }

    public bool Create(AuditLog entity, string? username = null)
    {
        try
        {
            entity.Active = true;
            entity.Deleted = false;
            entity.DateCreated = DateTime.UtcNow;
            entity.CreatedBy = username;
            if (bookazone.AuditLogs == null) return false;
            bookazone.AuditLogs.Add(entity);
            var result = bookazone.SaveChangesAsync().Result;
            return result > 0;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        return false;
    }

    public bool Update(AuditLog entity, string? username = null)
    {
        try
        {
            entity.DateUpdated = DateTime.UtcNow;
            entity.UpdatedBy = username;
            if (bookazone.AuditLogs == null) return false;
            bookazone.AuditLogs.Update(entity);
            var result = bookazone.SaveChangesAsync().Result;
            return result > 0;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }

        return false;
    }

    public bool Delete(int? id, string? username = null)
    {
        var entity = Find(id);
        if (id == null || entity == null) throw new Except(ErrorHttp.NotFound);
        entity.Active = false;
        entity.Deleted = true;
        entity.DateDeleted = DateTime.UtcNow;
        entity.DeletedBy = username;
        if (bookazone.AuditLogs == null) throw new Except(ErrorHttp.DbCreateError);
        bookazone.AuditLogs.Update(entity);
        var result = bookazone.SaveChangesAsync().Result;
        return result > 0;
    }

    public bool Status(int? id, bool active, string? username = null)
    {
        var entity = Find(id);
        if (id == null || entity == null) throw new Except(ErrorHttp.NotFound);
        entity.Active = active;
        entity.DateUpdated = DateTime.UtcNow;
        entity.UpdatedBy = username;
        if (bookazone.AuditLogs == null) throw new Except(ErrorHttp.DbCreateError);
        bookazone.AuditLogs.Update(entity);
        var result = bookazone.SaveChangesAsync().Result;
        return result > 0;
    }
}