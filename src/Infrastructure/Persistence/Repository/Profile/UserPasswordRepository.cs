using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Bookazone.Application.Interfaces.Repository.Profile;
using Bookazone.Domain.Entities.Profile;
using Bookazone.Infrastructure.Persistence.DbContext;

namespace Bookazone.Infrastructure.Persistence.Repository.Profile;

public class UserPasswordRepository(
    BookazoneDbContext db,
    IPasswordHasher<Users> hasher,
    ILogger<UserPasswordRepository> logger
) : IUserPasswordRepository
{

    /// <summary>
    /// Retrieves the current active password record for a given user.
    /// </summary>
    public async Task<UserPassword?> GetCurrentPasswordAsync(Guid userId)
    {
        return await db.UserPasswords
            .Where(p => p.FkUserId == userId && p.IsCurrent)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Retrieves recent password history for a user.
    /// </summary>
    public async Task<IEnumerable<UserPassword>> GetPasswordHistoryAsync(Guid userId, int limit = 5)
    {
        return await db.UserPasswords
            .AsNoTracking()
            .Where(p => p.FkUserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }

    /// <summary>
    /// Validates a plain password against the user's current password hash.
    /// If the stored hash needs rehashing (algorithm/iteration upgrade), it will rehash automatically.
    /// </summary>
    public async Task<bool> ValidatePasswordAsync(Guid userId, string plainPassword)
    {
        var record = await GetCurrentPasswordAsync(userId);
        if (record == null)
        {
            logger.LogWarning("No active password found for user {UserId}", userId);
            return false;
        }

        var user = await db.Users.FindAsync(userId);
        if (user == null)
        {
            logger.LogWarning("User {UserId} not found for password validation", userId);
            return false;
        }

        var verificationResult = hasher.VerifyHashedPassword(user, record.Password, plainPassword);

        if (verificationResult == PasswordVerificationResult.Failed)
            return false;

        // Update last used time
        record.LastUsedAt = DateTime.UtcNow;
        db.UserPasswords.Update(record);
        await db.SaveChangesAsync();

        // If the hasher indicates rehash is needed (e.g., iteration count increased), re-hash to the latest format
        if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            try
            {
                await RehashPasswordAsync(user, plainPassword, "system-auto-rehash");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Auto-rehash failed for user {UserId}", userId);
                // Don't fail validation because of rehash failure; user is authenticated.
            }
        }

        return true;
    }

    /// <summary>
    /// Creates and stores a new password for the user, marking old ones as inactive.
    /// This method will throw if the user does not exist.
    /// </summary>
    public async Task<UserPassword> CreatePasswordAsync(Guid userId, string plainPassword, string changedBy)
    {
        try
        {
            var user = await db.Users.FindAsync(userId);
            if (user == null) throw new InvalidOperationException($"User {userId} not found");
            if (await IsPasswordReusedAsync(userId, plainPassword, historyLimit: 6))
                throw new InvalidOperationException("This password was used recently. Choose a different password.");
            var existingCurrent = await db.UserPasswords
                .Where(p => p.FkUserId == userId && p.IsCurrent)
                .ToListAsync();

            if (existingCurrent.Any())
            {
                foreach (var old in existingCurrent)
                    old.IsCurrent = false;

                db.UserPasswords.UpdateRange(existingCurrent);
                await db.SaveChangesAsync();
            }
            var hash = hasher.HashPassword(user, plainPassword);
            var newPassword = new UserPassword
            {
                FkUserId = userId,
                Password = hash,
                CreatedAt = DateTime.UtcNow,
                IsCurrent = true,
                ExpiresAt = DateTime.UtcNow.AddMonths(6)
            };
            await db.UserPasswords.AddAsync(newPassword);
            await db.SaveChangesAsync();
            
            logger.LogInformation("Password updated successfully for user {UserId}", userId);
            return newPassword;
        }
        catch (Exception ex)
        {
            try {  } catch { /* swallow rollback errors */ }
            logger.LogError(ex, "Error creating new password for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Marks the current password as recently used (for auditing).
    /// </summary>
    public async Task<bool> MarkPasswordAsUsedAsync(Guid userId)
    {
        var current = await GetCurrentPasswordAsync(userId);
        if (current == null) return false;

        current.LastUsedAt = DateTime.UtcNow;
        db.UserPasswords.Update(current);
        await db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Checks whether the new password was used recently (prevents reuse).
    /// Compares against the most recent `historyLimit` password hashes.
    /// </summary>
    public async Task<bool> IsPasswordReusedAsync(Guid userId, string plainPassword, int historyLimit = 5)
    {
        var user = await db.Users.FindAsync(userId);
        if (user == null) return false;

        var recentPasswords = await GetPasswordHistoryAsync(userId, historyLimit);
        foreach (var p in recentPasswords)
        {
            var verification = hasher.VerifyHashedPassword(user, p.Password, plainPassword);
            if (verification == PasswordVerificationResult.Success || verification == PasswordVerificationResult.SuccessRehashNeeded)
            {
                logger.LogWarning("User {UserId} attempted to reuse a recent password", userId);
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Internal helper to re-hash a verified plain password into a new record (used when Hasher requests rehash).
    /// This preserves history by marking previous current as non-current.
    /// </summary>
    private async Task RehashPasswordAsync(Users user, string plainPassword, string changedBy)
    {
        // mark existing current as not current
        var existingCurrent = await db.UserPasswords
            .Where(p => p.FkUserId == user.Id && p.IsCurrent)
            .ToListAsync();

        foreach (var old in existingCurrent)
            old.IsCurrent = false;

        db.UserPasswords.UpdateRange(existingCurrent);
        await db.SaveChangesAsync();

        // create new hashed entry
        var newHash = hasher.HashPassword(user, plainPassword);
        var newPassword = new UserPassword
        {
            FkUserId = user.Id,
            Password = newHash,
            CreatedAt = DateTime.UtcNow,
            IsCurrent = true,
            ExpiresAt = DateTime.UtcNow.AddMonths(6),
            UpdatedBy = changedBy
        };

        await db.UserPasswords.AddAsync(newPassword);
        await db.SaveChangesAsync();

        logger.LogInformation("Rehashed password stored for user {UserId}", user.Id);
    }
}
