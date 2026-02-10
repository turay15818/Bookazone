using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.Interfaces.Repository.Profile;
using Bookazone.Domain.Entities.Profile;
using Bookazone.Infrastructure.Persistence.DbContext;


namespace Bookazone.Infrastructure.Persistence.Repository.Profile;

public class RefreshTokenRepository(BookazoneDbContext db) : IRefreshTokenRepository
{
    public async Task<RefreshToken> CreateAsync(Guid userId, Guid deviceId)
    {
        var rawToken = Guid.NewGuid().ToString("N") + Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var hashed = CryptoService.HashToken(rawToken);
        var refresh = new RefreshToken
        {
            FkDeviceId = deviceId,
            FkUserId = userId,
            Token = hashed,
            Active = true,
            Deleted = false,
            DateCreated = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(14),
        };
        db.RefreshTokens?.Add(refresh);
        await db.SaveChangesAsync();
        refresh.Token = rawToken;
        return refresh;
    }

    public async Task<RefreshToken?> GetAsync(string token)
    {
       // var hashed = CryptoService.HashToken(token);

            return await db.RefreshTokens
                .Include(x => x.FkUser)
                .Include(x => x.FkDevice)
                .FirstOrDefaultAsync(x => x.Token == token && !x.Revoked);
    }

    public async Task RevokeAsync(RefreshToken token)
    {
        token.Revoked = true;
        token.Deleted = true;
        token.Active = false;
        token.DateDeleted = DateTime.UtcNow;
        token.RevokedAt = DateTime.UtcNow;
        db.RefreshTokens?.Update(token);
        await db.SaveChangesAsync();
    }
    
    
    
    public async Task RevokeAllForUserAsync(Guid userId)
    {
        var tokens = db.RefreshTokens.Where(r => r.FkUserId == userId && !r.Revoked).ToList();
        foreach (var token in tokens)
        {
            token.Revoked = true;
            token.RevokedAt = DateTime.UtcNow;
        }
        await db.SaveChangesAsync();
    }

    public async Task RevokeAllForDeviceAsync(Guid deviceId)
    {
        var tokens = db.RefreshTokens.Where(r => r.FkDeviceId == deviceId && !r.Revoked).ToList();
        foreach (var token in tokens)
        {
            token.Revoked = true;
            token.RevokedAt = DateTime.UtcNow;
        }
        await db.SaveChangesAsync();
    }

    public async Task RevokeAllForTenantAsync(Guid tenantId)
    {
        var tokens = db.RefreshTokens
            .Include(r => r.FkUser)
            .Where(r => r.FkUser != null && r.FkUser.FkTenant != null && r.FkUser.FkTenant.Id == tenantId && !r.Revoked)
            .ToList();

        foreach (var token in tokens)
        {
            token.Revoked = true;
            token.RevokedAt = DateTime.UtcNow;
        }
        await db.SaveChangesAsync();
    }
    public async Task CleanUpOldTokensAsync(Guid userId, Guid deviceId)
    {
        var now = DateTime.UtcNow;
        var tokens = db.RefreshTokens
            .Where(t => t.FkUserId == userId && t.FkDeviceId == deviceId)
            .ToList();

        foreach (var token in tokens)
        {
            if (token.ExpiryDate < now)
            {
                token.Revoked = true;
                token.RevokedAt = now;
                token.RevokeReason = "Expired";
            }
        }

        // Ensure only one active token remains
        var activeTokens = tokens.Where(t => !t.Revoked).OrderByDescending(t => t.DateCreated).ToList();
        if (activeTokens.Count > 1)
        {
            foreach (var old in activeTokens.Skip(1))
            {
                old.Revoked = true;
                old.RevokedAt = now;
                old.RevokeReason = "Replaced by newer refresh token";
            }
        }

        await db.SaveChangesAsync();
    }
 
    
}


public class TokenRevocationService(
    IRefreshTokenRepository refreshTokenRepository,
    ILogger<TokenRevocationService> logger)
{
    public async Task RevokeAllUserTokensAsync(Guid userId, string reason = "User initiated")
    {
        await refreshTokenRepository.RevokeAllForUserAsync(userId);
        logger.LogInformation("🔒 Revoked all refresh tokens for user {UserId}. Reason: {Reason}", userId, reason);
    }

    public async Task RevokeDeviceTokensAsync(Guid deviceId, string reason = "Device removed")
    {
        await refreshTokenRepository.RevokeAllForDeviceAsync(deviceId);
        logger.LogInformation("🔒 Revoked all refresh tokens for device {DeviceId}. Reason: {Reason}", deviceId, reason);
    }

    public async Task RevokeTenantTokensAsync(Guid tenantId, string reason = "Tenant policy change")
    {
        await refreshTokenRepository.RevokeAllForTenantAsync(tenantId);
        logger.LogInformation("🔒 Revoked all refresh tokens for tenant {TenantId}. Reason: {Reason}", tenantId, reason);
    }
    
    

    
    
    
}
