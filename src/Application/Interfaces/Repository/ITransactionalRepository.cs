using Microsoft.EntityFrameworkCore.Storage;

namespace Bookazone.Application.Interfaces.Repository;

public interface ITransactionalRepository
{
    Task<IDbContextTransaction> BeginTransactionAsync();
}
