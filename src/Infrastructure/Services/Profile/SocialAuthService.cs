using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs;
using Bookazone.Application.Interfaces.Repository.Permission;
using Bookazone.Application.Interfaces.Repository.Profile;
using Bookazone.Application.Interfaces.Repository.Tenant;
using Bookazone.Application.Interfaces.Services;
using Bookazone.Application.Interfaces.Services.Profile;
using Bookazone.Domain.Entities.Profile;
using Microsoft.Extensions.Logging;

namespace Bookazone.Infrastructure.Services.Profile;

public class SocialAuthService(
    ILogger<SocialAuthService> logger,
    IGoogleTokenValidator googleValidator,
    IUserRepository userRepository,
    IAuthProviderRepository authProviderRepository,
    IUserTenantRepository userTenantRepository,
    ITenantRepository tenantRepository,
    IDeviceRepository deviceRepository,
    IUserRoleRepository userRoleRepository,
    IUserPermissionRepository userPermissionRepository,
    IUserPermissionService userPermissionService,
    IOneTimePasswordRepository otpRepository,
    IEmailService emailService,
    IAuthenticateService authenticateService,
    IRefreshTokenRepository refreshTokenRepository
) : ISocialAuthService
{
    private const string ProviderName = "Google";

    public async Task<dynamic> GoogleLoginAsync(SocialGoogleLoginRequest req, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(req.IdToken))
                return ApiResponse.Error(ErrorHttp.BadRequest, new Exception("Google idToken is required."));
            if (req.DeviceRequest == null || string.IsNullOrWhiteSpace(req.DeviceRequest.Identifier))
                return ApiResponse.Error(ErrorHttp.BadRequest, new Exception("Device information is required."));

            // ✅ 1) Validate Google token
            var payload = await googleValidator.ValidateIdTokenAsync(req.IdToken, ct);
            var providerLink = await authProviderRepository.GetByProviderAsync(ProviderName, payload.Subject, ct);
            Users? user = null;

            if (providerLink != null && providerLink.FkUserId != null)
            {
                user = userRepository.Find(providerLink.FkUserId.Value);
            }

            if (user == null)
            {
                user = await FindOrCreateUserAsync(payload, ct);
                if (providerLink == null)
                {
                    var existingLink = await authProviderRepository.GetByUserAndProviderAsync(user.Id, ProviderName, ct);
                    if (existingLink == null)
                    {
                        await authProviderRepository.AddAsync(new AuthProvider
                        {
                            FkUserId = user.Id,
                            Provider = ProviderName,
                            ProviderUserId = payload.Subject,
                            Email = payload.Email,
                            EmailVerified = payload.EmailVerified,
                            PictureUrl = payload.PictureUrl,
                            IsPrimary = true,
                            DateCreated = DateTime.UtcNow,
                            Active = true,
                            Deleted = false
                        }, ct);
                    }
                }
            }
            var memberships = await userTenantRepository.GetByUserIdAsync(user.Id, ct);

            var tenants = memberships
                .Where(m => true)
                .Select(m => new
                {
                    m.FkTenant!.Id,
                    m.FkTenant.Name,
                    m.FkTenant.Subdomain,
                    m.FkTenant.Logo,
                    Role = m.Role // enum or role name
                })
                .ToList();


            var activeTenant = await tenantRepository.GetByIdAsync(req.TenantId ?? Guid.Empty);
            var device = deviceRepository.FindByIdentifierAndUser(req.DeviceRequest.Identifier, user);

            if (device == null)
            {
                device = new Device
                {
                    Slug = Functions.GenerateUid(),
                    Name = req.DeviceRequest.Name,
                    Identifier = req.DeviceRequest.Identifier,
                    Version = req.DeviceRequest.Version,
                    Os = req.DeviceRequest.Os,
                    AppVersion = req.DeviceRequest.AppVersion,
                    Verified = true,
                    FkUser = user,
                    CreatedBy = user.Username ?? "system",
                    DateCreated = DateTime.UtcNow,
                    Active = true,
                    Deleted = false
                };

                deviceRepository.Create(device);
            }
            else
            {
                // optional: update device metadata / fcm token
                device.Name = req.DeviceRequest.Name ?? device.Name;
                device.Os = req.DeviceRequest.Os ?? device.Os;
                device.Version = req.DeviceRequest.Version ?? device.Version;
                device.AppVersion = req.DeviceRequest.AppVersion ?? device.AppVersion;
                device.DateUpdated = DateTime.UtcNow;
                device.NotificationToken = req.NotificationToken ?? device.NotificationToken;    
                deviceRepository.Update(device);
            }

            // ✅ 7) If device is not verified → OTP flow (same behavior)
            /*if (device.Verified != true)
            {
                var otpResponse = await otpRepository.GenerateOtp(new BookazoneOtpGenerate
                {
                    Email = user.Email,
                    Purpose = "Device Verification",
                    IsTokenBased = true,
                    DeviceRequest =  
                    {
                        Identifier = req.DeviceRequest.Identifier,
                        Name = req.DeviceRequest.Name,
                        Os = req.DeviceRequest.Os,
                        Version = req.DeviceRequest.Version,
                        AppVersion = req.DeviceRequest.AppVersion
                    }
                });

                if (otpResponse == null)
                    return ApiResponse.Error(ErrorHttp.OtpInvalid);

                var verificationLink = $"http://localhost:5173/identity-verification?token={otpResponse.Token}";
                var emailBody = emailService.GenerateDeviceVerificationEmailHtml(user.Firstname ?? "User", verificationLink);
                await emailService.SendEmailAsync(user.Email!, "Verify New Device", emailBody);

                return ApiResponse.Success(new
                {
                    message = "We sent a verification link to your email. Please verify this device before logging in.",
                    reference = otpResponse.Reference,
                    device = new { device.Id, device.Identifier, device.Name },
                    user = new { user.Id, user.Email }
                });
            }*/

            // ✅ 8) Update last authenticated
            user.TryCount = 0;
            user.LastTryCount = null;
            user.LastAuthenticated = DateTime.UtcNow;
            userRepository.Update(user);

            // ✅ 9) Generate JWT + refresh token (same as normal login)
            var (jwt, expiresInSeconds, sessionId) =
                await authenticateService.GenerateJwtForUserAsync(user, device, activeTenant);

            var refreshToken = await refreshTokenRepository.CreateAsync(user.Id, device.Id);

            device.CurrentSessionToken = CryptoService.Encrypt(jwt);
            device.MemorySlug = sessionId;
            device.LastConnect = DateTime.UtcNow;
            deviceRepository.Update(device);

            // ✅ 10) Roles + permissions (same as normal login)
            var permissions = userPermissionRepository.FindByUserId(user.Id)
                .Where(p => p.FkPermission?.Name != null)
                .Select(p => p.FkPermission!.Name)
                .ToList();

            var roles = userRoleRepository.FindByUserId(user.Id)
                .Where(r => r.FkRole?.Name != null)
                .Select(r => r.FkRole!.Name)
                .ToList();

            var response = new
            {
                tenants,
                user = new
                {
                    user.Id,
                    user.Email,
                    user.Firstname,
                    user.Lastname,
                    user.Address,
                    user.ProfileImage,
                    user.LastAuthenticated,
                    roles,
                    permissions
                },
                device = new { device.Id, device.Identifier, verified = device.Verified },
                auth = new
                {
                    access_token = jwt,
                    expires_in = expiresInSeconds,
                    refresh_token = refreshToken.Token,
                    refresh_expires_in = (refreshToken.ExpiryDate - DateTime.UtcNow).TotalSeconds,
                    session_id = sessionId
                }
            };

            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Google login error");
            return  ex.Message;
        }
    }

    private async Task<Users> FindOrCreateUserAsync(GoogleTokenPayload payload, CancellationToken ct)
    {
        // Only link by email if verified
        if (payload.EmailVerified && !string.IsNullOrWhiteSpace(payload.Email))
        {
            var existing = userRepository.FindByEmailOrPhoneOrUsername(payload.Email)?.FirstOrDefault();
            if (existing != null) return existing;
        }

        var newUser = new Users
        {
            Email = payload.Email,
            Username = payload.Email,
            EmailVerified = payload.EmailVerified,
            Firstname = payload.GivenName,
            Lastname = payload.FamilyName,
            ProfileImage = payload.PictureUrl,
            LastAuthenticated = DateTime.UtcNow,
            Active = true,
            Deleted = false,
            DateCreated = DateTime.UtcNow
        };

        await userRepository.CreateAsync(newUser)!;
        
        
        await userPermissionService.SetupCustomerAsync(newUser, newUser.Username ?? newUser.Email ?? "system");

        
        return newUser;
    }
}