using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.Interfaces.Repository.Permission;
using Bookazone.Infrastructure.Persistence.DbContext;


namespace Bookazone.Infrastructure.Persistence.Repository.Permission;

public class PermissionRepository(BookazoneDbContext context, ILogger<PermissionRepository> logger) 
    : IPermissionRepository
{
    public async Task<List<Domain.Entities.Permissions.Permission>> GetAllAsync()
    {
        return await context.Permissions
            .Where(p => !p.Deleted && p.Active)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Domain.Entities.Permissions.Permission?> GetByIdAsync(Guid id)
    {
        return await context.Permissions
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && !p.Deleted);
    }
    
    public async Task<Domain.Entities.Permissions.Permission?> GetBySlugAsync(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug)) return null;

        return await context.Permissions
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Slug == slug && !p.Deleted);
    }

    public async Task<Domain.Entities.Permissions.Permission?> GetByNameAsync(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;

        return await context.Permissions
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Name == name && !p.Deleted);
    }

    public async Task<Domain.Entities.Permissions.Permission> CreateAsync(
        Domain.Entities.Permissions.Permission entity)
    {
        var existing = await GetBySlugAsync(entity.Slug);
        if (existing != null)
            throw new Except(ErrorHttp.DbCreateError, "A permission with the same slug already exists.");

        entity.Active = true;
        entity.Deleted = false;
        entity.DateCreated = DateTime.UtcNow;

        context.Permissions.Add(entity);
        await context.SaveChangesAsync();

        logger.LogInformation("Permission created: {Name} ({Slug})", entity.Name, entity.Slug);
        return entity;
    }

    public async Task<Domain.Entities.Permissions.Permission> UpdateAsync(
        Domain.Entities.Permissions.Permission entity)
    {
        var existing = await GetByIdAsync(entity.Id);
        if (existing == null)
            throw new Except(ErrorHttp.NotFound);

        entity.DateUpdated = DateTime.UtcNow;
        context.Permissions.Update(entity);
        await context.SaveChangesAsync();

        logger.LogInformation("Permission updated: {Name} ({Slug})", entity.Name, entity.Slug);
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null)
            throw new Except(ErrorHttp.NotFound);

        entity.Active = false;
        entity.Deleted = true;
        entity.DateDeleted = DateTime.UtcNow;

        context.Permissions.Update(entity);
        await context.SaveChangesAsync();

        logger.LogInformation("Permission deleted: {Id}", id);
        return true;
    }

    public async Task<bool> ExistsBySlugAsync(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug)) return false;

        return await context.Permissions
            .AnyAsync(p => p.Slug == slug && !p.Deleted);
    }
}