using Bookazone.Application.Interfaces.Repository.Profile;
using Bookazone.Domain.Entities.Profile;
using Bookazone.Infrastructure.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Bookazone.Infrastructure.Persistence.Repository.Profile;

public class AuthProviderRepository(BookazoneDbContext db) : IAuthProviderRepository
{
    public Task<AuthProvider?> GetByProviderAsync(string provider, string providerUserId, CancellationToken ct = default)
        => db.AuthProviders.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Provider == provider && x.ProviderUserId == providerUserId && x.Active && !x.Deleted, ct);

    public Task<AuthProvider?> GetByUserAndProviderAsync(Guid userId, string provider, CancellationToken ct = default)
        => db.AuthProviders.AsNoTracking()
            .FirstOrDefaultAsync(x => x.FkUserId == userId && x.Provider == provider && x.Active && !x.Deleted, ct);

    public async Task AddAsync(AuthProvider entity, CancellationToken ct = default)
    {
        db.AuthProviders.Add(entity);
        await db.SaveChangesAsync(ct);
    }
}