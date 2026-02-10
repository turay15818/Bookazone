using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Bookazone.Application.Interfaces.Repository;
using Bookazone.Domain.Common;
using Bookazone.Infrastructure.Persistence.DbContext;

namespace Bookazone.Infrastructure.Persistence.Repository;

public abstract class BaseRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly BookazoneDbContext Context;
    protected readonly ILogger Logger;

    protected BaseRepository(BookazoneDbContext context, ILogger logger)
    {
        Context = context;
        Logger = logger;
    }

    public IQueryable<T> Query() => Context.Set<T>().AsQueryable();

    public virtual async Task<IEnumerable<T>> GetAllAsync() =>
        await Context.Set<T>().Where(e => !e.Deleted).ToListAsync();

    public virtual async Task<T?> GetByIdAsync(Guid id) =>
        await Context.Set<T>().FirstOrDefaultAsync(e => e.Id == id && !e.Deleted);

    public virtual async Task<T> AddAsync(T entity)
    {
        entity.Active = true;
        entity.Deleted = false;
        await Context.Set<T>().AddAsync(entity);
        await Context.SaveChangesAsync();
        Logger.LogInformation("{Entity} added successfully", typeof(T).Name);
        return entity;
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        entity.DateUpdated = DateTime.UtcNow;
        Context.Set<T>().Update(entity);
        await Context.SaveChangesAsync();
        Logger.LogInformation("{Entity} updated successfully", typeof(T).Name);
        return entity;
    }

    public virtual async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await Context.Set<T>().FindAsync(id);
        if (entity == null) return false;

        entity.Deleted = true;
        entity.DateDeleted = DateTime.UtcNow;
        Context.Set<T>().Update(entity);
        await Context.SaveChangesAsync();

        Logger.LogInformation("{Entity} with ID {Id} marked as deleted", typeof(T).Name, id);
        return true;
    }

    public virtual async Task<bool> ExistsAsync(Guid id)
        => await Context.Set<T>().AnyAsync(e => e.Id == id && !e.Deleted);

    public async Task<IDbContextTransaction> BeginTransactionAsync()
        => await Context.Database.BeginTransactionAsync();
}

