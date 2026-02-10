using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.Interfaces.Repository.Permission;
using Bookazone.Domain.Entities.Permissions;
using Bookazone.Infrastructure.Persistence.DbContext;


namespace Bookazone.Infrastructure.Persistence.Repository.Permission;

public class PermissionGroupRepository(
        BookazoneDbContext dbContext,
        ILogger<PermissionGroupRepository> logger
    ) : IPermissionGroupRepository
    {
        public async Task<List<PermissionGroup>> AllAsync()
        {
            if (dbContext.PermissionGroups == null)
                throw new Except(ErrorHttp.DbQueryRunFailed);

            return await dbContext.PermissionGroups
                .Where(pg => pg.Deleted != true)
                .Include(pg => pg.Permission)
                .AsNoTracking()
                .ToListAsync();
        }

        public List<PermissionGroup> All()
        {
            if (dbContext.PermissionGroups == null)
                throw new Except(ErrorHttp.DbQueryRunFailed);

            return dbContext.PermissionGroups
                .Where(pg => pg.Deleted != true)
                .Include(pg => pg.Permission)
                .AsNoTracking()
                .ToList();
        }

        public async Task<PermissionGroup?> FindAsync(Guid? id)
        {
            if (dbContext.PermissionGroups == null)
                throw new Except(ErrorHttp.DbQueryRunFailed);

            return await dbContext.PermissionGroups
                .Include(pg => pg.Permission)
                .AsNoTracking()
                .FirstOrDefaultAsync(pg => pg.Id == id && pg.Deleted != true);
        }

        public PermissionGroup? Find(Guid? id)
        {
            if (dbContext.PermissionGroups == null)
                throw new Except(ErrorHttp.DbQueryRunFailed);

            return dbContext.PermissionGroups
                .Include(pg => pg.Permission)
                .AsNoTracking()
                .FirstOrDefault(pg => pg.Id == id && pg.Deleted != true);
        }

        public PermissionGroup? FindByName(string? name)
        {
            if (dbContext.PermissionGroups == null)
                throw new Except(ErrorHttp.DbQueryRunFailed);

            return name != null
                ? dbContext.PermissionGroups
                    .Include(pg => pg.Permission)
                    .AsNoTracking()
                    .FirstOrDefault(pg => pg.Name == name && pg.Deleted != true)
                : null;
        }

        public async Task<PermissionGroup?> CreateAsync(PermissionGroup entity)
        {
            if (dbContext.PermissionGroups == null)
                throw new Except(ErrorHttp.DbCreateError);

            entity.Active = true;
            entity.Deleted = false;
            entity.DateCreated = DateTime.UtcNow;

            dbContext.PermissionGroups.Add(entity);
            var result = await dbContext.SaveChangesAsync();
            return result > 0 ? entity : null;
        }

        public PermissionGroup? Create(PermissionGroup entity)
        {
            var check = FindByName(entity.Name);
            if (check != null)
                throw new Except(ErrorHttp.AlreadyExists);

            if (dbContext.PermissionGroups == null)
                throw new Except(ErrorHttp.DbCreateError);

            entity.Active = true;
            entity.Deleted = false;
            entity.DateCreated = DateTime.UtcNow;

            dbContext.PermissionGroups.Add(entity);
            var result = dbContext.SaveChanges();
            return result > 0 ? entity : null;
        }

        public async Task<PermissionGroup?> UpdateAsync(PermissionGroup entity)
        {
            if (dbContext.PermissionGroups == null)
                throw new Except(ErrorHttp.DbCreateError);

            entity.DateUpdated = DateTime.UtcNow;
            dbContext.PermissionGroups.Update(entity);
            var result = await dbContext.SaveChangesAsync();
            return result > 0 ? entity : null;
        }

        public PermissionGroup? Update(PermissionGroup entity)
        {
            if (dbContext.PermissionGroups == null)
                throw new Except(ErrorHttp.DbCreateError);

            entity.DateUpdated = DateTime.UtcNow;
            dbContext.PermissionGroups.Update(entity);
            var result = dbContext.SaveChanges();
            return result > 0 ? entity : null;
        }

        public async Task<bool> DeleteAsync(Guid? id)
        {
            var entity = await FindAsync(id);
            if (id == null || entity == null)
                throw new Except(ErrorHttp.NotFound);

            entity.Active = false;
            entity.Deleted = true;
            entity.DateDeleted = DateTime.UtcNow;

            if (dbContext.PermissionGroups == null)
                throw new Except(ErrorHttp.DbCreateError);

            dbContext.PermissionGroups.Update(entity);
            var result = await dbContext.SaveChangesAsync();
            return result > 0;
        }

        public bool Delete(Guid? id)
        {
            var entity = Find(id);
            if (id == null || entity == null)
                throw new Except(ErrorHttp.NotFound);

            entity.Active = false;
            entity.Deleted = true;
            entity.DateDeleted = DateTime.UtcNow;

            if (dbContext.PermissionGroups == null)
                throw new Except(ErrorHttp.DbCreateError);

            dbContext.PermissionGroups.Update(entity);
            var result = dbContext.SaveChanges();
            return result > 0;
        }

        public async Task<bool> StatusAsync(Guid? id, bool active)
        {
            var entity = await FindAsync(id);
            if (id == null || entity == null)
                throw new Except(ErrorHttp.NotFound);

            entity.Active = active;
            entity.DateUpdated = DateTime.UtcNow;

            dbContext.PermissionGroups.Update(entity);
            var result = await dbContext.SaveChangesAsync();
            return result > 0;
        }

        public bool Status(Guid? id, bool active)
        {
            var entity = Find(id);
            if (id == null || entity == null)
                throw new Except(ErrorHttp.NotFound);

            entity.Active = active;
            entity.DateUpdated = DateTime.UtcNow;

            dbContext.PermissionGroups.Update(entity);
            var result = dbContext.SaveChanges();
            return result > 0;
        }
    }