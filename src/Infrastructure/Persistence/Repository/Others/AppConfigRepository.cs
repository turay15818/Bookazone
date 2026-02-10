using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.Interfaces.Repository.Others;
using Bookazone.Domain.Entities.Others;
using Bookazone.Infrastructure.Persistence.DbContext;


namespace Bookazone.Infrastructure.Persistence.Repository.Others;

public class AppConfigRepository(BookazoneDbContext BookazoneDbContext) : IAppConfigRepository
{
    public List<AppConfig> All()
    {
        return (BookazoneDbContext.AppConfig ?? throw new Except(ErrorHttp.DbQueryRunFailed))
            .Where(a => a.Deleted == false).ToListAsync().Result;
    }

    public AppConfig? Find(Guid? id)
    {
        if (BookazoneDbContext.AppConfig == null) throw new Except(ErrorHttp.DbQueryRunFailed);
        return BookazoneDbContext.AppConfig.FindAsync(id).Result;
    }

    public AppConfig? FindBySlug(string? slug)
    {
        if (BookazoneDbContext.AppConfig == null) throw new Except(ErrorHttp.DbQueryRunFailed);
        return slug == null
            ? null
            : BookazoneDbContext.AppConfig.FirstOrDefault(a =>
                a.Slug == slug && a.Active == true && a.Deleted != true);
    }

    public AppConfig? Create(AppConfig entity)
    {
        entity.Active = true;
        entity.Deleted = false;
        entity.DateCreated = DateTime.UtcNow;
        if (BookazoneDbContext.AppConfig == null) throw new Except(ErrorHttp.DbCreateError);
        BookazoneDbContext.AppConfig.Add(entity);
        var result = BookazoneDbContext.SaveChangesAsync().Result;
        return result > 0 ? entity : null;
    }

    public AppConfig? Update(AppConfig entity)
    {
        entity.DateUpdated = DateTime.UtcNow;
        entity.UpdatedBy = "USER";
        if (BookazoneDbContext.AppConfig == null) throw new Except(ErrorHttp.DbCreateError);
        BookazoneDbContext.AppConfig.Update(entity);
        var result = BookazoneDbContext.SaveChangesAsync().Result;
        return result > 0 ? entity : null;
    }

    public bool Delete(Guid? id)
    {
        var entity = Find(id);
        if (id == null || entity == null) throw new Except(ErrorHttp.NotFound);
        entity.Active = false;
        entity.Deleted = true;
        entity.DateDeleted = DateTime.UtcNow;
        entity.DeletedBy = "USER";
        if (BookazoneDbContext.AppConfig == null) throw new Except(ErrorHttp.DbCreateError);
        BookazoneDbContext.AppConfig.Update(entity);
        var result = BookazoneDbContext.SaveChangesAsync().Result;
        return result > 0;
    }

    public bool Status(Guid? id, bool active)
    {
        var entity = Find(id);
        if (id == null || entity == null) throw new Except(ErrorHttp.NotFound);
        entity.Active = active;
        entity.DateUpdated = DateTime.UtcNow;
        entity.UpdatedBy = "USER";
        if (BookazoneDbContext.AppConfig == null) throw new Except(ErrorHttp.DbCreateError);
        BookazoneDbContext.AppConfig.Update(entity);
        var result = BookazoneDbContext.SaveChangesAsync().Result;
        return result > 0;
    }
}