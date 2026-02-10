using Microsoft.EntityFrameworkCore.Storage;
using Bookazone.Domain.Entities.Tenant;

namespace Bookazone.Application.Interfaces.Repository;

public interface IGenericRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(Guid id);
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    IQueryable<T> Query(); 
    Task<IDbContextTransaction> BeginTransactionAsync();
}

public interface IUnitOfWork
{
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

