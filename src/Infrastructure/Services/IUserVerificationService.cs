using System.IdentityModel.Tokens.Jwt;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Bookazone.Application.Common.Shared.Helpers;
using Bookazone.Domain.Entities.Profile;
using Bookazone.Infrastructure.Persistence;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Services.JWT;

namespace Bookazone.Infrastructure.Services;

using Microsoft.Extensions.Logging;

public interface IUserVerificationService
{
    // Task<string> CreateAndStoreVerificationTokenAsync(Users user, string fingerprintHash, TimeSpan ttl);
    Task<(bool Verified, string? Error, bool ChallengeIssued)> VerifyTokenAsync(string rawToken, string requestFingerprint, string requestIp, string? deviceIdentifier, string? memorySlug);
    Task<bool> ValidateChallengeOtpAsync(Guid otpId, string numericOtp, string requestFingerprint);
}

public class UserVerificationService(
    BookazoneDbContext db,
    ITokenService tokenService,
    IEmailService emailService,
    ILogger<UserVerificationService> logger,
    IOptions<JwtVerificationOptions> jwtOpts)
    : IUserVerificationService
{
    // implement earlier SendEmailAsync
    private readonly ILogger<UserVerificationService> _logger = logger;
    private readonly TimeSpan _defaultTtl = TimeSpan.FromMinutes(jwtOpts.Value.ExpiryMinutes > 0 ? jwtOpts.Value.ExpiryMinutes : 60);

    // 1) Create JWT and save hash to OneTimePassword
    public async Task<string> CreateAndStoreVerificationTokenAsync(Users user, string fingerprintHash, TimeSpan? ttl = null)
    {
        var otp = new OneTimePassword
        {
            FkUser = user,
            Email = user.Email,
            Purpose = "ACCOUNT_VERIFICATION",
            IsTokenBased = true,
            Used = false,
            ExpiredDate = DateTime.UtcNow.Add(ttl ?? _defaultTtl),
            DateCreated = DateTime.UtcNow
        };

        db.OneTimePassword.Add(otp);
        await db.SaveChangesAsync(); // Ensure otp.Id exists

        var token = tokenService.CreateVerificationToken(user.Id, otp.Id, fingerprintHash, ttl ?? _defaultTtl);

        // store hash of token (so DB doesn't keep raw token)
        otp.Token = SecurityHelpers.ComputeSha256Hash(token);
        await db.SaveChangesAsync();

        return token;
    }

    // 2) Validate token, fingerprint and optionally issue challenge
    public async Task<(bool Verified, string? Error, bool ChallengeIssued)> VerifyTokenAsync(string rawToken, string requestFingerprint, string requestIp, string? deviceIdentifier, string? memorySlug)
    {
        // 1. Validate signature and expiration
        var principal = tokenService.ValidateToken(rawToken, validateLifetime: true);
        if (principal == null) return (false, "Invalid or expired token", false);

        // extract claims
        var sub = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        var otpIdStr = principal.FindFirst("otpId")?.Value;
        var fpClaim = principal.FindFirst("fp")?.Value;

        if (!Guid.TryParse(sub, out var userId) || !Guid.TryParse(otpIdStr, out var otpId))
            return (false, "Malformed token", false);

        // fetch DB OTP record
        var otp = await db.OneTimePassword
            .Include(o => o.FkUser)
            .FirstOrDefaultAsync(o => o.Id == otpId);

        if (otp == null) return (false, "Token record not found", false);
        if (otp.Used == true) return (false, "Token already used", false);
        if (otp.ExpiredDate.HasValue && otp.ExpiredDate.Value < DateTime.UtcNow) return (false, "Token expired", false);

        // verify token matches DB stored hash
        var tokenHash = SecurityHelpers.ComputeSha256Hash(rawToken);
        if (tokenHash != otp.Token) return (false, "Token mismatch", false);

        // At this point JWT is valid and token matches DB
        // compute request fingerprint (server-side)
        var computedFp = requestFingerprint; // already should be hashed by controller helper

        // Compare fingerprint claim (from token) against computed
        if (string.Equals(fpClaim, computedFp, StringComparison.Ordinal))
        {
            // success path: mark used, mark user verified, mark device trusted
            otp.Used = true;
            otp.UsedTimeDate = DateTime.UtcNow;
            await db.SaveChangesAsync();

            var user = otp.FkUser!;
            user.EmailVerified = true;
            user.UpdatedBy = "system";
            user.DateUpdated = DateTime.UtcNow;
            await db.SaveChangesAsync();

            // Optionally mark device as verified if a matching device exists (lookup by deviceIdentifier / memorySlug)
            if (!string.IsNullOrWhiteSpace(deviceIdentifier) || !string.IsNullOrWhiteSpace(memorySlug))
            {
                var device = await db.Devices.FirstOrDefaultAsync(d =>
                    d.FkUser != null && d.FkUser.Id == user.Id &&
                    (d.Identifier == deviceIdentifier || d.MemorySlug == memorySlug));

                if (device != null)
                {
                    device.Verified = true;
                    device.DateVerified = DateTime.UtcNow;
                    await db.SaveChangesAsync();
                }
            }

            return (true, null, false);
        }
        else
        {
            var numericOtp = new Random().Next(100000, 999999).ToString();
            var challenge = new OneTimePassword
            {
                FkUser = otp.FkUser,
                Email = otp.Email,
                Purpose = "DEVICE VERIFICATION CHALLENGE",
                Otp = numericOtp,
                IsTokenBased = false,
                Used = false,
                ExpiredDate = DateTime.UtcNow.AddMinutes(10),
                DateCreated = DateTime.UtcNow
            };
            db.OneTimePassword.Add(challenge);
            await db.SaveChangesAsync();

            var body = emailService.GenerateDeviceChallengeEmailHtml(otp.FkUser!.Firstname ?? "User", numericOtp);
            await emailService.SendEmailAsync(otp.Email!, "Device verification required", body);

            return (false, "Device mismatch — challenge issued", true);
        }
    }

    // Validate numeric OTP challenge
    public async Task<bool> ValidateChallengeOtpAsync(Guid otpId, string numericOtp, string requestFingerprint)
    {
        var record = await db.OneTimePassword.Include(o => o.FkUser).FirstOrDefaultAsync(o => o.Id == otpId);
        if (record == null) return false;
        if (record.Used == true) return false;
        if (record.ExpiredDate.HasValue && record.ExpiredDate.Value < DateTime.UtcNow) return false;
        if (record.Otp != numericOtp) return false;

        // mark used
        record.Used = true;
        record.UsedTimeDate = DateTime.UtcNow;
        await db.SaveChangesAsync();
        var user = record.FkUser!;
        user.EmailVerified = true;
        await db.SaveChangesAsync();
        return true;
    }
}
