using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookazone.Api.Controllers.V1.Config;
using Bookazone.Api.Controllers.V1.Config.Request;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs;
using Bookazone.Application.Interfaces.Repository.Others;
using Bookazone.Application.Interfaces.Repository.Permission;
using Bookazone.Application.Interfaces.Repository.Profile;
using Bookazone.Application.Interfaces.Repository.Tenant;
using Bookazone.Application.Interfaces.Services.Profile;
using Bookazone.Domain.Entities.Profile;
using Bookazone.Infrastructure.Services;
using Bookazone.Infrastructure.Services.JWT;
using Functions = Bookazone.Application.Common.Shared.Utils.Functions;

namespace Bookazone.Api.Controllers.V1.Security;

[ApiController]
[ApiExplorerSettings(GroupName = SwaggerConfig.SwaggerDocName.WebVersion1.Slug)]
[Route(RouteWebVersion1.Security.Auth.Base)]
public class AuthController(
    ILogger<AuthController> logger,
    IUserRepository userRepository,
    IPermissionRepository permissionRepository,
    IUserTenantRepository userTenantRepository,
    IUserPermissionRepository userPermissionRepository,
    IUserRoleRepository userRoleRepository,
    IDeviceRepository deviceRepository,
    ITenantRepository tenantRepository,
    IUserPasswordRepository userPasswordRepository,
    IAuthenticateService authenticateService,
    IOneTimePasswordRepository otpRepository,
    IEmailService emailService,
    IJwtService tokenService,
    TokenRevocationService tokenRevocationService,
    Functions functions,
    IRefreshTokenRepository refreshTokenRepository,
    ISocialAuthService socialAuthService,
    IAppConfigRepository appConfigRepository
) : ControllerBase
{
    [HttpPost]
    [Route(RouteWebVersion1.Security.Auth.CheckUsername)]
    public async Task<ApiResult> CheckUsername(UsernameCheckRequest authRequest)
    {
        try
        {
            if (string.IsNullOrEmpty(authRequest.DeviceRequest?.Identifier))
                throw new Except(ErrorHttp.BadRequest);
            if (authRequest.Username == null)
                throw new Except(ErrorHttp.NotEncoded);

           
            return await Task.FromResult(
                authenticateService.UsernameCheck(authRequest.Username, authRequest.DeviceRequest)
            );
        }
        catch (Exception e)
        {
            logger.LogError("error");
            return await Task.FromResult(ApiResponse.Error(ErrorHttp.Error, e));
        }
    }
    
    [HttpPost]
    [Route(RouteWebVersion1.Security.Auth.Google)]
    public async Task<ApiResult> GoogleLogin([FromBody] SocialGoogleLoginRequest req)
    {
        try
        {
            var authenticate = socialAuthService.GoogleLoginAsync(req).Result;
            return ApiResponse.Success(authenticate);
    
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error during google login");
            return ApiResponse.Error(ErrorHttp.Error, e);
        }
    }

    

 [HttpPost]
[Route(RouteWebVersion1.Security.Auth.Login)]
public async Task<ApiResult> LoginUser([FromBody] UserLoginRequest authRequest)
{
    try
    {
        logger.LogInformation("🔐 Login attempt for username: {Username}", authRequest?.Username);
        if (string.IsNullOrWhiteSpace(authRequest?.Username) || string.IsNullOrWhiteSpace(authRequest.Password))
            return ApiResponse.Error(ErrorHttp.BadRequest, new Exception("Username and password are required."));
        if (string.IsNullOrEmpty(authRequest.DeviceRequest.Identifier))
            return ApiResponse.Error(ErrorHttp.BadRequest, new Exception("Device information is required."));

        // ✅ 2. Find user
        var user = userRepository.FindByEmailOrPhoneOrUsername(authRequest.Username)?.FirstOrDefault();
        if (user == null)
            return ApiResponse.Error(ErrorHttp.UserNotFound);

        // ✅ 3. Check tenant validity
        var tenant = user.FkTenant != null ? await tenantRepository.GetByNameAsync(user.FkTenant.Name) : null;
        
        var memberships = await userTenantRepository.GetByUserIdAsync(user.Id);

        var tenants =  memberships
            .Where(m => m.FkTenant != null)
            .Select(m => new
            {
                m.FkTenant.Id,
                m.FkTenant.Name,
                m.FkTenant.Subdomain,
                m.FkTenant.Logo,
                Role = m.Role // enum or role name
            })
            .ToList();
        
        /*if (tenant == null) return ApiResponse.Error(ErrorHttp.NotFound, new Exception("Associated tenant not found."));
        if (tenant.Active !=true && tenant.IsVerified !=true) return ApiResponse.Error(ErrorHttp.NotFound, new Exception("Associated tenant not found."));*/
        // ✅ 4. Verify password
        var passwordValid = await userPasswordRepository.ValidatePasswordAsync(user.Id, authRequest.Password);
        if (!passwordValid)
        {
            user.TryCount++;
            user.LastTryCount = DateTime.UtcNow;
            userRepository.Update(user);
            return ApiResponse.Error(ErrorHttp.PasswordNotMatch);
        }

        // ✅ 5. Retrieve or create device record
        var device = deviceRepository.FindByIdentifierAndUser(authRequest.DeviceRequest.Identifier, user);
        if (device == null)
        {
            device = new Device
            {
                Slug = Functions.GenerateUid(),
                Name = authRequest.DeviceRequest.Name,
                Identifier = authRequest.DeviceRequest.Identifier,
                Version = authRequest.DeviceRequest.Version,
                Os = authRequest.DeviceRequest.Os,
                AppVersion = authRequest.DeviceRequest.AppVersion,
                Verified = false,
                FkUser = user,
                CreatedBy = user.Username ?? "system",
                DateCreated = DateTime.UtcNow,
            };
            deviceRepository.Create(device);
        }

        // ✅ 6. If device is not verified → send OTP

        if (device.Verified != true)
        {
            var otpResponse = await otpRepository.GenerateOtp(new BookazoneOtpGenerate
            {
                Email = user.Email,
                Purpose = "Device Verification",
                IsTokenBased = true,
                DeviceRequest = new DeviceRequest 
                {
                    Identifier = authRequest.DeviceRequest.Identifier,
                    Name = authRequest.DeviceRequest.Name,
                    Os = authRequest.DeviceRequest.Os,
                    Version = authRequest.DeviceRequest.Version,
                    AppVersion = authRequest.DeviceRequest.AppVersion
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
        }
        user.TryCount = 0;
        user.LastTryCount = null;
        user.LastAuthenticated = DateTime.UtcNow;
        userRepository.Update(user);

        var (jwt, expiresInSeconds, sessionId) = await authenticateService.GenerateJwtForUserAsync(user, device, tenant);
        var refreshToken = await refreshTokenRepository.CreateAsync(user.Id, device.Id);

        device.CurrentSessionToken = CryptoService.Encrypt(jwt);
        device.MemorySlug = sessionId;
        device.NotificationToken = authRequest.NotificationToken;
        device.LastConnect = DateTime.UtcNow;
        deviceRepository.Update(device);

        var permissions =  userPermissionRepository.FindByUserId(user.Id)
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
            user = new { user.Id, user.Email, user.Firstname, user.Lastname, user.Address, user.ProfileImage, user.LastAuthenticated, roles, permissions },
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

        return ApiResponse.Success(response);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Login error for username {Username}", authRequest?.Username);
        return ApiResponse.Error(ErrorHttp.Error, ex);
    }
}


 [HttpPost]
    [Route(RouteWebVersion1.Security.Auth.LoginBiometric)]
    public async Task<ApiResult> LoginBiometric(BiometricAuthRequest authBiometricRequest)
    {
        try
        {
            if (string.IsNullOrEmpty(authBiometricRequest.DeviceRequest.Identifier))
                throw new Except(ErrorHttp.BadRequest);
            if (
                authBiometricRequest.Username == null ||
                authBiometricRequest.BiometricId == null
            ) throw new Except(ErrorHttp.NotEncoded);
            
            if (authBiometricRequest.Username == null || authBiometricRequest.BiometricId == null)
                return await Task.FromResult(ApiResponse.Error(ErrorHttp.BiometricUnset));

            var users = userRepository.FindByEmailOrPhoneOrUsername(authBiometricRequest.Username);
            if (users is null or { Count: 0 })
                return await Task.FromResult(ApiResponse.Error(ErrorHttp.UserNotFound));

            var user = users.First();

            /*
            var appStatus = appConfigRepository.FindBySlug(Consts.AppConfig.AppStatusKeyV2);
            if (appStatus?.Value != Consts.AppStatus.Active && user.AppStatusException != true)
                return await Task.FromResult(ApiResponse.Error(ErrorHttp.AppInactive));
                */

            if (user.Frozen == true) return await Task.FromResult(ApiResponse.Error(ErrorHttp.UserFrozen));
            if (user.PasswordExpired == true)
                return await Task.FromResult(ApiResponse.Error(ErrorHttp.PasswordExpired));

            var device = deviceRepository.ListByIdentifierAndUser(authBiometricRequest.DeviceRequest.Identifier, user)?
                .FirstOrDefault(a =>
                    a is { Verified: true, Biometric: true } && a.BiometricKey == authBiometricRequest.BiometricId);
            if (device == null) return await Task.FromResult(ApiResponse.Error(ErrorHttp.BiometricUnset));

            if ((user.Locked == true || user.TryCount >= 3) && user.TryCount != null &&
                user.LastTryCount > DateTime.Now.AddMinutes(-15))
                return await Task.FromResult(ApiResponse.Error(ErrorHttp.UserLocked));

            var checkCustomerEmail = userRepository.FindByEmailOrPhoneOrUsername(user.Email)?.FirstOrDefault();
            var checkCustomerPhone = userRepository.FindByEmailOrPhoneOrUsername(user.Phone)?.FirstOrDefault();
            if (checkCustomerEmail != null && checkCustomerPhone != null &&
                checkCustomerEmail.Id != checkCustomerPhone.Id)
                return await Task.FromResult(ApiResponse.Error(ErrorHttp.PhoneEmailExistingForDiffCustomer));

            device.NotificationToken = authBiometricRequest.NotificationToken;

            // Update user's location if provided
            if (authBiometricRequest.Longitude.HasValue && authBiometricRequest.Latitude.HasValue)
            {
                user.Longitude = authBiometricRequest.Longitude;
                user.Latitude = authBiometricRequest.Latitude;
                userRepository.Update(user);
                logger.LogInformation("Updated user location via biometric login: Longitude {Longitude}, Latitude {Latitude}", 
                    authBiometricRequest.Longitude, authBiometricRequest.Latitude);
            }

           
            var tenant = user.FkTenant != null ? await tenantRepository.GetByNameAsync(user.FkTenant.Name) : null;
        
            var memberships = await userTenantRepository.GetByUserIdAsync(user.Id);

            var tenants =  memberships
                .Where(m => m.FkTenant != null)
                .Select(m => new
                {
                    m.FkTenant.Id,
                    m.FkTenant.Name,
                    m.FkTenant.Subdomain,
                    m.FkTenant.Logo,
                    Role = m.Role // enum or role name
                })
                .ToList();

            
            
            var (jwt, expiresInSeconds, sessionId) = await authenticateService.GenerateJwtForUserAsync(user, device, tenant);
            var refreshToken = await refreshTokenRepository.CreateAsync(user.Id, device.Id);

            device.CurrentSessionToken = CryptoService.Encrypt(jwt);
            device.MemorySlug = sessionId;
            device.LastConnect = DateTime.UtcNow;
            deviceRepository.Update(device);

            var permissions =  userPermissionRepository.FindByUserId(user.Id)
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
                user = new { user.Id, user.Email, user.Firstname, user.Lastname, user.Address, user.ProfileImage, user.LastAuthenticated, roles, permissions },
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

            return ApiResponse.Success(response); 
            
            
            
            
        }
        catch (Exception e)
        {
            logger.LogError("error");
            return await Task.FromResult(ApiResponse.Error(ErrorHttp.Error, e));
        }
    }

 
 
 
 
 
 
 

[HttpPost]
[Route(RouteWebVersion1.Security.Auth.InviteAccept)]
public async Task<ApiResult> AcceptInvitation([FromBody] InviteAcceptRequest request)
{
    try
    {
        if (string.IsNullOrEmpty(request.Token))
            return ApiResponse.Error(ErrorHttp.BadRequest, new Exception("Missing invitation token."));

        var validToken = await otpRepository.GetByTokenAsync(request.Token);
        if (validToken == null) return ApiResponse.Error(ErrorHttp.NotFound);
        
        // ✅ Decode token
        /*
        var decoded = authenticateService.DecodeJwt(request.Token);
        if (decoded == null)
            return ApiResponse.Error(ErrorHttp.Unauthorized, new Exception("Invalid or malformed token."));

        if (decoded["purpose"]?.ToString() != "User Invitation")
            return ApiResponse.Error(ErrorHttp.Unauthorized, new Exception("Invalid token purpose."));
            */

        var userId = validToken.FkUser?.Id.ToString();
      //  var tenantId = decoded["tenant_id"]?.ToString();
        var reference = validToken.Reference;

        /*
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(reference))
            return ApiResponse.Error(ErrorHttp.BadRequest, new Exception("Invalid token payload."));*/
        // ✅ Validate user
        var user = userRepository.Find(new Guid(userId));
        if (user == null) return ApiResponse.Error(ErrorHttp.UserNotFound);
      //  if (user.Active) return ApiResponse.Error(ErrorHttp.BadRequest, new Exception("This invitation has already been accepted."));

        // ✅ Lookup OTP record
        var otpRecord = otpRepository.FindByReference(reference);
        if (otpRecord == null)
            return ApiResponse.Error(ErrorHttp.NotFound, new Exception("Invitation record not found."));
        /*if (string.IsNullOrEmpty(otpRecord.Token) || 
            CryptoService.HashToken(request.Token) != otpRecord.Token)
        {
            return ApiResponse.Error(ErrorHttp.Unauthorized, new Exception("Invalid or mismatched invitation token."));
        }
        if (string.IsNullOrEmpty(otpRecord.Token) || otpRecord.Token != request.Token)
            return ApiResponse.Error(ErrorHttp.Unauthorized, new Exception("Invalid or mismatched invitation token."));*/

        if (otpRecord.Used)
            return ApiResponse.Error(ErrorHttp.Forbidden, new Exception("This invitation token has already been used."));
        if (otpRecord.ExpiredDate < DateTime.UtcNow)
            return ApiResponse.Error(ErrorHttp.TokenExpired, new Exception("This invitation has expired."));
        otpRecord.Used = true;
        otpRecord.Active = false;
        otpRecord.UsedTimeDate = DateTime.UtcNow;
        otpRepository.Update(otpRecord);
        var device = deviceRepository.FindByIdentifierAndUser(request.DeviceRequest.Identifier, user);
        if (device == null)
        {
            device = new Device
            {
                Slug = Functions.GenerateUid(),
                Identifier = request.DeviceRequest.Identifier,
                Name = request.DeviceRequest.Name,
                Os = request.DeviceRequest.Os,
                System = request.DeviceRequest.System,
                AppVersion = request.DeviceRequest.AppVersion,
                Version = request.DeviceRequest.Version,
                Verified = false,
                FkUser = user,
                DateCreated = DateTime.UtcNow
            };
            deviceRepository.Create(device);
        }
        var setupClaims = new List<Claim>
        {
            new Claim("purpose", "Password Setup"),
            new Claim("user_id", user.Id.ToString()),
            new Claim("device_id", device.Id.ToString())
        };
        var setupToken = authenticateService.GenerateJwt(setupClaims, expiresInMinutes: 15);

        return ApiResponse.Success(new
        {
            message = "Invitation accepted successfully. Proceed to set your password.",
            setup_token = setupToken,
            expires_in = 900
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error accepting invitation");
        return ApiResponse.Error(ErrorHttp.Error, ex);
    }
}


[HttpPost]
[Route(RouteWebVersion1.Security.Auth.InviteSetPassword)]
public async Task<ApiResult> InviteSetPassword([FromBody] SetPasswordRequest request)
{
    try
    {
        if (string.IsNullOrEmpty(request.Token))
            return ApiResponse.Error(ErrorHttp.BadRequest, new Exception("Missing token."));

        var decoded = tokenService.DecodeJwt(request.Token);
        if (decoded == null)
            return ApiResponse.Error(ErrorHttp.Unauthorized, new Exception("Invalid or malformed token."));

        if (decoded["purpose"]?.ToString() != "Password Setup")
            return ApiResponse.Error(ErrorHttp.Unauthorized, new Exception("Invalid token purpose."));

        var userId = decoded["user_id"]?.ToString();
        var deviceId = decoded["device_id"]?.ToString();

        var user = userRepository.Find(new Guid(userId));
        var device = deviceRepository.Find(new Guid(deviceId));

        if (user == null || device == null)
            return ApiResponse.Error(ErrorHttp.NotFound);

        if (!authenticateService.IsStrongPassword(request.Password))
            return ApiResponse.Error(ErrorHttp.WeakPassword, new Exception("Weak password."));

        await userPasswordRepository.CreatePasswordAsync(user.Id, request.Password, user.Email ?? "system");

        user.Active = true;
        user.EmailVerified = true;
        userRepository.Update(user);

        device.Verified = true;
        device.DateVerified = DateTime.UtcNow;
        deviceRepository.Update(device);

        return ApiResponse.Success(new
        {
            message = "Your account is now active. You can log in."
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error setting invited user password");
        return ApiResponse.Error(ErrorHttp.Error, ex);
    }
}


[HttpPost]
[Route(RouteWebVersion1.Security.Auth.RefreshToken)]
public async Task<ApiResult> RefreshToken([FromBody] RefreshTokenRequest request)
{
    try
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            return ApiResponse.Error(ErrorHttp.BadRequest, new Exception("Refresh token is required."));

        if (request.DeviceRequest == null || string.IsNullOrEmpty(request.DeviceRequest.Identifier))
            return ApiResponse.Error(ErrorHttp.BadRequest, new Exception("Device information is required."));

        // 🔐 Step 1: Lookup token
        var storedToken = await refreshTokenRepository.GetAsync(request.Token);
        if (storedToken == null)
            return ApiResponse.Error(ErrorHttp.Unauthorized, new Exception("Invalid refresh token."));

        if (storedToken.ExpiryDate < DateTime.UtcNow)
            return ApiResponse.Error(ErrorHttp.TokenExpired, new Exception("Refresh token has expired."));

        var user = storedToken.FkUser;
        var device = storedToken.FkDevice;
        if (user == null || device == null)
            return ApiResponse.Error(ErrorHttp.UserNotFound);

        // 🧩 Step 2: Validate device match
        if (!string.Equals(device.Identifier, request.DeviceRequest.Identifier, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("⚠️ Device mismatch for refresh token: Expected={Expected}, Received={Received}",
                device.Identifier, request.DeviceRequest.Identifier);

            return ApiResponse.Error(ErrorHttp.DeviceNotVerified,
                new Exception("Device mismatch — refresh token not valid for this device."));
        }

        // Optional: you can also check other device attributes
        if (!string.Equals(device.Os, request.DeviceRequest.Os, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(device.AppVersion, request.DeviceRequest.AppVersion, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("⚠️ Device attribute mismatch on refresh token");
            return ApiResponse.Error(ErrorHttp.DeviceVerificationRequired,
                new Exception("Device information mismatch."));
        }

        // ✅ Step 3: Validate tenant
        var tenant = user.FkTenant != null
            ? await tenantRepository.GetByNameAsync(user.FkTenant.Name)
            : null;

    
        // ✅ Step 4: Rotate tokens (invalidate old, issue new)
        var (newJwt, expiresInSeconds, newSessionId) =
            await authenticateService.GenerateJwtForUserAsync(user, device, tenant);

        var newRefresh = await refreshTokenRepository.CreateAsync(user.Id, device.Id);

        storedToken.Revoked = true;
        storedToken.ReplacedByToken = newRefresh.Token;
        storedToken.RevokedAt = DateTime.UtcNow;
        await refreshTokenRepository.RevokeAsync(storedToken);

        logger.LogInformation("✅ Refresh token rotated successfully for user {UserId} on device {DeviceId}",
            user.Id, device.Id);

        return ApiResponse.Success(new
        {
            message = "Token refreshed successfully.",
            access_token = newJwt,
            expires_in = expiresInSeconds,
            refresh_token = newRefresh.Token,
            refresh_expires_in = (newRefresh.ExpiryDate - DateTime.UtcNow).TotalSeconds,
            device = new
            {
                device.Id,
                device.Identifier,
                device.Name,
                device.Os,
                device.AppVersion
            }
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Error refreshing token");
        return ApiResponse.Error(ErrorHttp.Error, ex);
    }
}



[Authorize]
[HttpPost]
[Route(RouteWebVersion1.Security.Auth.LogoutAll)]
public async Task<ApiResult> LogoutAll()
{
    var user = functions.GetUser(User.Identity?.Name);
    if (user == null) return ApiResponse.Error(ErrorHttp.Unauthorized);
    await tokenRevocationService.RevokeAllUserTokensAsync(user.Id, "User requested logout from all devices");
    return ApiResponse.Success(new { message = "All sessions have been logged out successfully." });
}


[HttpPost]
[Route(RouteWebVersion1.Security.Auth.PasswordReset)]
public async Task<ApiResult> PasswordReset(PasswordResetRequest passwordResetRequest)
{
    try
    {
        if (string.IsNullOrEmpty(passwordResetRequest.DeviceRequest?.Identifier))
            throw new Except(ErrorHttp.BadRequest);

        if (string.IsNullOrEmpty(passwordResetRequest.Username))
            return ApiResponse.Error(ErrorHttp.BadRequest, new Exception("Username or email is required."));

        var users = userRepository.FindByEmailOrPhoneOrUsername(passwordResetRequest.Username);
        if (users is null or { Count: 0 or > 1 })
            return ApiResponse.Error(ErrorHttp.UserNotFound);

        var user = users.First();
        if (user.Email == null && user.Phone == null)
            return ApiResponse.Error(ErrorHttp.UserWithoutEmailPhone);

        var latestOtp = await otpRepository.GetLatestForUserAsync(user.Id, "Reset Password");
        if (latestOtp != null && latestOtp.DateCreated.HasValue &&
            (DateTime.UtcNow - latestOtp.DateCreated.Value).TotalMinutes < 5)
        {
            return ApiResponse.Error(ErrorHttp.TooManyRequests,
                new Exception("A reset link or code was recently sent. Please check your email."));
        }

        var otpResponse = await otpRepository.GenerateOtp(new BookazoneOtpGenerate
        {
            Email = user.Email,
            Phone = user.Phone,
            Purpose = "Reset Password",
            IsTokenBased = passwordResetRequest.UseToken,
        });

        if (otpResponse == null)
            return ApiResponse.Error(ErrorHttp.OtpInvalid, new Exception("Could not generate OTP."));

        var devices = deviceRepository.ListByIdentifierAndUser(passwordResetRequest.DeviceRequest.Identifier, user);
        Device device = devices?.FirstOrDefault();

        if (device == null)
        {
            device = new Device
            {
                Slug = Functions.GenerateUid(),
                Name = passwordResetRequest.DeviceRequest.Name,
                Identifier = passwordResetRequest.DeviceRequest.Identifier,
                Version = passwordResetRequest.DeviceRequest.Version,
                System = passwordResetRequest.DeviceRequest.System,
                Os = passwordResetRequest.DeviceRequest.Os,
                AppVersion = passwordResetRequest.DeviceRequest.AppVersion,
                FkUser = user,
                Verified = false,
                DateCreated = DateTime.UtcNow,
                OtpEmailReference = otpResponse.Reference,
                OtpPhoneReference = otpResponse.Reference
            };

            deviceRepository.Create(device);
        }
        else
        {
            device.OtpEmailReference = otpResponse.Reference;
            device.OtpPhoneReference = otpResponse.Reference;
            deviceRepository.Update(device);
        }

        // Send email
        var resetLink = $"http://localhost:5173/verify-reset?token={otpResponse.Token}";
        var html = emailService.GeneratePasswordResetEmailHtml(user.Firstname ?? "User", resetLink);
        await emailService.SendEmailAsync(user.Email, "Password Reset Request", html);

        return ApiResponse.Success(new
        {
            message = $"A verification link has been sent to {user.Email}.",
            purpose = "Reset Password",
            reference = otpResponse.Reference,
            expires_in = "10 minutes"
        });
    }
    catch (Exception e)
    {
        logger.LogError(e, "Error during password reset request");
        return ApiResponse.Error(ErrorHttp.Error, e);
    }
}

[HttpPost]
[Route(RouteWebVersion1.Security.Auth.VerifyReset)]
public async Task<ApiResult> VerifyReset([FromBody] VerifyResetRequest request)
{
    try
    {
        if (string.IsNullOrEmpty(request.Token) && string.IsNullOrEmpty(request.Otp))
            return ApiResponse.Error(ErrorHttp.BadRequest, new Exception("Either token or OTP must be provided."));

        Users? user = null;
        OneTimePassword? otpRecord = null;
        if (!string.IsNullOrEmpty(request.Token))
        {
            otpRecord = await otpRepository.GetByTokenAsync(request.Token);
            if (otpRecord == null)
                return ApiResponse.Error(ErrorHttp.Unauthorized, new Exception("Invalid or expired reset token."));
            if (otpRecord.Token != request.Token)
                return ApiResponse.Error(ErrorHttp.Unauthorized, new Exception("Token mismatch."));
            if (otpRecord.Used)
                return ApiResponse.Error(ErrorHttp.Forbidden, new Exception("This reset link has already been used."));
            if (otpRecord.ExpiredDate < DateTime.UtcNow)
                return ApiResponse.Error(ErrorHttp.OtpExpired, new Exception("Reset link expired."));
            var decoded = tokenService.DecodeJwt(request.Token);
            if (decoded == null)
                return ApiResponse.Error(ErrorHttp.Unauthorized, new Exception("Malformed reset token."));

            var purpose = decoded["purpose"]?.ToString();
            var userId = decoded["user_id"]?.ToString();
            var reference = decoded["reference"]?.ToString();

            if (purpose != "Reset Password")
                return ApiResponse.Error(ErrorHttp.Unauthorized, new Exception("Invalid token purpose."));
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(reference))
                return ApiResponse.Error(ErrorHttp.BadRequest, new Exception("Invalid token payload."));
            if (request.Reference != otpRecord.Reference)
                return ApiResponse.Error(ErrorHttp.Unauthorized, new Exception("Reference does not match token."));
            user = otpRecord.FkUser ?? userRepository.Find(new Guid(userId));
            if (user == null || user.Id.ToString() != userId)
                return ApiResponse.Error(ErrorHttp.Unauthorized, new Exception("User mismatch."));
        }
        else
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Reference))
                return ApiResponse.Error(ErrorHttp.BadRequest, new Exception("Username and reference are required."));

            user = userRepository.FindByEmailOrPhoneOrUsername(request.Username)?.FirstOrDefault();
            if (user == null)
                return ApiResponse.Error(ErrorHttp.UserNotFound);

            otpRecord = otpRepository.FindByReference(request.Reference);
            if (otpRecord == null)
                return ApiResponse.Error(ErrorHttp.OtpInvalid);
            if (otpRecord.Used)
                return ApiResponse.Error(ErrorHttp.OtpInvalid, new Exception("OTP already used."));
            if (otpRecord.Otp != request.Otp)
                return ApiResponse.Error(ErrorHttp.OtpInvalid, new Exception("Invalid OTP."));
            if (otpRecord.ExpiredDate < DateTime.UtcNow)
                return ApiResponse.Error(ErrorHttp.OtpExpired, new Exception("OTP expired."));

            if (otpRecord.FkUser?.Id != user.Id)
                return ApiResponse.Error(ErrorHttp.Unauthorized, new Exception("OTP does not belong to this user."));
        }
        otpRecord.Used = true;
        otpRecord.Active = false;
        otpRecord.UsedTimeDate = DateTime.UtcNow;
        otpRepository.Update(otpRecord);
        var claims = new List<Claim>
        {
            new Claim("purpose", "Password Reset Finalization"),
            new Claim("user_id", user.Id.ToString()),
            new Claim("email", user.Email ?? ""),
            new Claim("reset_reference", otpRecord.Reference ?? "")
        };

        var resetToken = tokenService.GenerateJwt(claims, expiresInMinutes: 15);

        return ApiResponse.Success(new
        {
            message = "Verification successful. You can now set a new password.",
            reset_token = resetToken,
            expires_in = 900
        });
    }
    catch (Exception e)
    {
        logger.LogError(e, "Error verifying password reset");
        return ApiResponse.Error(ErrorHttp.Error, e);
    }
}


[HttpPost]
[Route(RouteWebVersion1.Security.Auth.PasswordResetValidate)]
public async Task<ApiResult> PasswordResetValidate([FromBody] ChangePasswordRequest data)
{
    await using var transaction = await otpRepository.BeginTransactionAsync();

    try
    {
        // 1️⃣ Input validation
        if (string.IsNullOrWhiteSpace(data?.Token) ||
            string.IsNullOrWhiteSpace(data?.Password) ||
            string.IsNullOrWhiteSpace(data?.ConfPassword))
            return ApiResponse.Error(ErrorHttp.BadRequest, new Exception("Token and passwords are required."));

        if (data.Password != data.ConfPassword)
            return ApiResponse.Error(ErrorHttp.PasswordNotMatch, new Exception("Passwords do not match."));

        // 2️⃣ Decode and verify token
        var decoded = tokenService.DecodeJwt(data.Token);
        if (decoded == null)
            return ApiResponse.Error(ErrorHttp.Unauthorized, new Exception("Invalid or malformed token."));

        if (decoded["purpose"]?.ToString() != "Password Reset Finalization")
            return ApiResponse.Error(ErrorHttp.Unauthorized, new Exception("Invalid token purpose."));

        var userId = decoded["user_id"]?.ToString();
        var reference = decoded["reset_reference"]?.ToString();

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(reference))
            return ApiResponse.Error(ErrorHttp.BadRequest, new Exception("Invalid token payload."));

        // 3️⃣ Load user
        var user = userRepository.Find(new Guid(userId));
        if (user == null)
            return ApiResponse.Error(ErrorHttp.UserNotFound);

        // 4️⃣ (Optional) Validate device
        var device = await deviceRepository.GetByIdentifierAsync(data.DeviceRequest.Identifier, user.Id);
        
        if (!authenticateService.IsStrongPassword(data.Password))
            return ApiResponse.Error(ErrorHttp.WeakPassword, new Exception(
                "Password must be at least 8 characters long, contain uppercase, lowercase, number, and special character."
            ));

        // 6️⃣ Check password reuse
        var reused = await userPasswordRepository.IsPasswordReusedAsync(user.Id, data.Password);
        if (reused)
            return ApiResponse.Error(ErrorHttp.PasswordReused, new Exception("You cannot reuse a previous password."));

        // 7️⃣ Set new password
        await userPasswordRepository.CreatePasswordAsync(user.Id, data.Password, user.Email ?? "system");

        // 8️⃣ Unlock account if necessary
        user.Locked = false;
        user.TryCount = 0;
        user.LastTryCount = null;
        userRepository.Update(user);

        await transaction.CommitAsync();
        await tokenRevocationService.RevokeAllUserTokensAsync(user.Id, "Password changed");
        logger.LogInformation("Password reset successfully for user {UserId}", user.Id);

        return ApiResponse.Success(new
        {
            message = "Password reset successfully. You can now log in."
        });
    }
    catch (Exception e)
    {
        await transaction.RollbackAsync();
        logger.LogError(e, "Error during password reset validation");
        return ApiResponse.Error(ErrorHttp.Error, e);
    }
}


[HttpPost]
[Route(RouteWebVersion1.Security.Auth.OtpValidate)]
public async Task<ApiResult> ValidateOtp([FromBody] OtpValidationRequest data)
{
    try
    {
        if (string.IsNullOrEmpty(data.Username) ||
            string.IsNullOrEmpty(data.Reference))
            return ApiResponse.Error(ErrorHttp.BadRequest, new Exception("Incomplete request."));

        var user = userRepository.FindByEmailOrPhoneOrUsername(data.Username)?.FirstOrDefault();
        if (user == null)
            return ApiResponse.Error(ErrorHttp.UserNotFound, new Exception("User not found."));

        var otpRecord = otpRepository.FindByReference(data.Reference);
        if (otpRecord == null)
            return ApiResponse.Error(ErrorHttp.OtpInvalid, new Exception("OTP reference invalid."));

        if(otpRecord.Used == true)return ApiResponse.Error(ErrorHttp.Unauthorized, new Exception("OTP already used."));
        if (otpRecord.ExpiredDate < DateTime.UtcNow)
            return ApiResponse.Error(ErrorHttp.OtpExpired, new Exception("OTP expired."));

        // If it's token-based
        if (otpRecord.IsTokenBased)
        {
            if (string.IsNullOrEmpty(data.Token) || otpRecord.Token != data.Token)
                return ApiResponse.Error(ErrorHttp.OtpInvalid, new Exception("Invalid verification token."));
        }
        else
        {
            if (string.IsNullOrEmpty(data.Otp) || otpRecord.Otp != data.Otp)
                return ApiResponse.Error(ErrorHttp.OtpInvalid, new Exception("Invalid OTP code."));
        }

        otpRecord.Active = false;
        otpRecord.Used = true;
        otpRecord.UsedTimeDate = DateTime.UtcNow;
        otpRepository.Update(otpRecord);

        // Update device (mark verified)
        var device = deviceRepository.FindByIdentifierAndUser(data.DeviceRequest?.Identifier, user);
        if (device != null)
        {
            device.Verified = true;
            if (data.Location != null)
            {
                device.Latitude = data.Location.Latitude;
                device.Longitude = data.Location.Longitude;
            }
            deviceRepository.Update(device);
        }

        // Generate a verification JWT (30 mins)
        var token = authenticateService.GenerateVerificationToken(user, device);

        return ApiResponse.Success(new
        {
            Message = "OTP verified successfully.",
            VerificationToken = token,
            ExpiresIn = 1800
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error validating OTP");
        return ApiResponse.Error(ErrorHttp.Error, ex);
    }
}

[HttpPost]
[Route(RouteWebVersion1.Security.Auth.DeviceValidate)]
public async Task<ApiResult> DeviceValidate([FromBody] OtpValidationRequest data)
{
    try
    {
        // 🔍 Detect verification type
        bool isTokenBased = !string.IsNullOrEmpty(data.Token);
        bool isOtpBased = !string.IsNullOrEmpty(data.Otp);

        if (!isTokenBased && !isOtpBased)
            return ApiResponse.Error(ErrorHttp.BadRequest, new Exception("Either a token or an OTP must be provided."));

        Users? user = null;
        Device? device = null;
        OneTimePassword? otpRecord = null;
        string? reference = data.Reference;

        // ✅ 1️⃣ Token-based verification (from link)
        if (isTokenBased)
        {
            if (data.Token != null)
            {
                var decoded = tokenService.DecodeJwt(data.Token);
                Console.WriteLine($"This is the decoded token: {decoded}");

                if (decoded["purpose"]?.ToString() != "Device Verification")
                    return ApiResponse.Error(ErrorHttp.Unauthorized, new Exception("Token purpose mismatch."));
                string hashedToken = CryptoService.HashToken(data.Token);
                
                var userId = decoded["user_id"]?.ToString();
                var deviceIdentifier = decoded["device_identifier"]?.ToString();
                reference = decoded["reference"]?.ToString();

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(deviceIdentifier))
                    return ApiResponse.Error(ErrorHttp.BadRequest, new Exception("Invalid token payload."));

                user = userRepository.Find(new Guid(userId));
                if (user == null)
                    return ApiResponse.Error(ErrorHttp.UserNotFound);

                device = deviceRepository.FindByIdentifierAndUser(deviceIdentifier, user);
            }

            if (device == null)
                return ApiResponse.Error(ErrorHttp.DeviceNotFound);
        }
        // ✅ 2️⃣ OTP-based verification
        else if (isOtpBased)
        {
            if (string.IsNullOrEmpty(data.Username) || string.IsNullOrEmpty(data.Reference))
                return ApiResponse.Error(ErrorHttp.BadRequest, new Exception("Username and reference are required for OTP verification."));

            user = userRepository.FindByEmailOrPhoneOrUsername(data.Username)?.FirstOrDefault();
            if (user == null)
                return ApiResponse.Error(ErrorHttp.UserNotFound);

            device = deviceRepository.FindByIdentifierAndUser(data.DeviceRequest?.Identifier, user);
            if (device == null)
                return ApiResponse.Error(ErrorHttp.DeviceNotFound);

            otpRecord = otpRepository.FindByReference(data.Reference);
            if (otpRecord == null)
                return ApiResponse.Error(ErrorHttp.OtpInvalid, new Exception("Invalid OTP reference."));

            if (otpRecord.Used)
                return ApiResponse.Error(ErrorHttp.Unauthorized, new Exception("OTP already used."));
            if (otpRecord.ExpiredDate < DateTime.UtcNow)
                return ApiResponse.Error(ErrorHttp.OtpExpired, new Exception("OTP expired."));

            if (otpRecord.Otp != data.Otp)
                return ApiResponse.Error(ErrorHttp.OtpInvalid, new Exception("Invalid OTP code."));
        }

        // ✅ 3️⃣ Final OTP lookup (for token-based case)
        if (otpRecord == null && !string.IsNullOrEmpty(reference))
            otpRecord = otpRepository.FindByReference(reference);

        if (otpRecord == null)
            otpRecord = await otpRepository.GetLatestForUserAsync(user!.Id, "");

        if (otpRecord == null)
            return ApiResponse.Error(ErrorHttp.OtpInvalid, new Exception("Verification record not found."));

        if (otpRecord.ExpiredDate < DateTime.UtcNow)
            return ApiResponse.Error(ErrorHttp.OtpExpired, new Exception("Verification code expired."));

        // ✅ 4️⃣ Mark OTP as used
        otpRecord.Used = true;
        otpRecord.Active = false;
        otpRecord.UsedTimeDate = DateTime.UtcNow;
        otpRepository.Update(otpRecord);

        // ✅ 5️⃣ Verify the device
        device!.Verified = true;
        device.DateVerified = DateTime.UtcNow;

        if (data.Location != null)
        {
            device.Latitude = data.Location.Latitude;
            device.Longitude = data.Location.Longitude;
        }

        deviceRepository.Update(device);

        // ✅ 6️⃣ Generate final JWT (real login token)
      
        var tenant = user!.FkTenant != null ? await tenantRepository.GetByNameAsync(user.FkTenant.Name) : null;
        var memberships = await userTenantRepository.GetByUserIdAsync(user.Id);

        var tenants =  memberships
            .Where(m => m.FkTenant != null)
            .Select(m => new
            {
                m.FkTenant.Id,
                m.FkTenant.Name,
                m.FkTenant.Subdomain,
                m.FkTenant.Logo,
                Role = m.Role // enum or role name
            })
            .ToList();
        /*if (tenant == null)
            return ApiResponse.Error(ErrorHttp.NotFound, new Exception("Associated tenant not found."));
        if (!tenant.Active || !tenant.IsVerified)
            return ApiResponse.Error(ErrorHttp.NotFound, new Exception("Tenant not active or verified."));*/
          

        var (jwt, expiresInSeconds, sessionId) = await authenticateService.GenerateJwtForUserAsync(user, device, tenant ?? null);
        var permissions =  userPermissionRepository.FindByUserId(user.Id)
            .Where(p => p.FkPermission?.Name != null)
            .Select(p => p.FkPermission!.Name)
            .ToList();

        var roles = userRoleRepository.FindByUserId(user.Id)
            .Where(r => r.FkRole?.Name != null)
            .Select(r => r.FkRole!.Name)
            .ToList();

        // ✅ 8️⃣ Response
        var response = new
        {
            message = "Device verified successfully. You are now logged in.",
            tenants, 
            user = new { user.Id, user.Email, user.Firstname, user.Lastname, user.Address, user.LastAuthenticated, roles, permissions },
            device = new { device.Id, device.Identifier, verified = device.Verified },
            auth = new { access_token = jwt, expires_in = expiresInSeconds, session_id = sessionId }
        };

        return ApiResponse.Success(response);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error verifying device via token or OTP");
        return ApiResponse.Error(ErrorHttp.Error, ex);
    }
}

[HttpPost]
[Route(RouteWebVersion1.Security.Auth.ResendVerification)]
public async Task<ApiResult> ResendVerification([FromBody] ResendVerificationRequest data)
{
    try
    {
        // ✅ 1. Validate request
        if (string.IsNullOrWhiteSpace(data.Username))
            return ApiResponse.Error(ErrorHttp.BadRequest, new Exception("Username or email is required."));

        if (data.DeviceRequest == null || string.IsNullOrEmpty(data.DeviceRequest.Identifier))
            return ApiResponse.Error(ErrorHttp.BadRequest, new Exception("Device information is required."));

        // ✅ 2. Find user
        var user = userRepository.FindByEmailOrPhoneOrUsername(data.Username)?.FirstOrDefault();
        if (user == null)
            return ApiResponse.Error(ErrorHttp.UserNotFound, new Exception("User not found."));

        // ✅ 3. Find device associated with this user and identifier
        var device = deviceRepository.FindByIdentifierAndUser(data.DeviceRequest.Identifier, user);
        if (device == null)
        {
            device = new Device
            {
                Slug = Functions.GenerateUid(),
                Name = data.DeviceRequest.Name,
                Identifier = data.DeviceRequest.Identifier,
                Version = data.DeviceRequest.Version,
                System = data.DeviceRequest.System,
                Os = data.DeviceRequest.Os,
                AppVersion = data.DeviceRequest.AppVersion,
                FkUser = user,
                Verified = false,
                DateCreated = DateTime.UtcNow,
                CreatedBy = user.Username ?? "system"
            };
            deviceRepository.Create(device);
        }

        // ✅ 4. Rate limiting — prevent resend spam
        var latestOtp = await otpRepository.GetLatestForUserAsync(user.Id, "Device Verification");
        if (latestOtp != null && latestOtp.DateCreated.HasValue && 
            (DateTime.UtcNow - latestOtp.DateCreated.Value).TotalMinutes < 2)
        {
            return ApiResponse.Error(ErrorHttp.TooManyRequests, 
                new Exception("Please wait at least 2 minutes before requesting another verification code."));
        }

        // ✅ 5. Invalidate old OTPs for this purpose
        await otpRepository.DeactivateAllForUserPurposeAsync(user.Id, "Device Verification");

        // ✅ 6. Generate new OTP or token
        var otpResponse = await otpRepository.GenerateOtp(new BookazoneOtpGenerate
        {
            Email = user.Email,
            Phone = user.Phone,
            Purpose = "Device Verification",
            IsTokenBased = data.UseToken,
            DeviceRequest = data.DeviceRequest
        });

        if (otpResponse == null)
            return ApiResponse.Error(ErrorHttp.Error, new Exception("Unable to generate verification code."));

        // ✅ 7. Send verification method
        if (data.UseToken)
        {
            var verificationLink = $"http://localhost:5173/identity-verification?token={otpResponse.Token}";
            var emailBody = emailService.GenerateDeviceVerificationEmailHtml(user.Firstname ?? "User", verificationLink);
            await emailService.SendEmailAsync(user.Email, "Verify your device", emailBody);
        }
        else
        {
            await emailService.SendOtpEmailAsync(user.Email, otpResponse.Otp ?? "", "Device Verification", otpResponse.Reference);
        }

        // ✅ 8. Update device metadata
        device.OtpEmailReference = otpResponse.Reference;
        device.OtpPhoneReference = otpResponse.Reference;
        deviceRepository.Update(device);

        // ✅ 9. Return response
        var message = data.UseToken
            ? "A new verification link has been sent to your email."
            : "A new OTP code has been sent to your email or phone.";

        return ApiResponse.Success(new
        {
            message,
            purpose = "Device Verification",
            reference = otpResponse.Reference,
            expires_in = otpResponse.ExpiredDate,
            device = new
            {
                device.Id,
                device.Identifier,
                device.Verified
            }
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error resending verification for {Username}", data.Username);
        return ApiResponse.Error(ErrorHttp.Error, ex);
    }
}

[HttpPost]
[Route(RouteWebVersion1.Security.Auth.IssueToken)]
[Authorize]
public async Task<ApiResult> IssueSessionToken()
{
    try
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var deviceId = User.FindFirstValue("device_id");
        var purpose = User.FindFirstValue("purpose");

        if (purpose != "verification")
            return ApiResponse.Error(ErrorHttp.Unauthorized, new Exception("Invalid token purpose."));

        var user =  userRepository.Find(Guid.Parse(userId));
        var device = deviceRepository.Find(Guid.Parse(deviceId));

        if (device == null || device.Verified != true)
            return ApiResponse.Error(ErrorHttp.DeviceNotVerified);

        // 🔑 Generate full session JWT
        var (jwt, expiresIn, sessionId) = authenticateService.GenerateSessionToken(user, device);
        await tokenRevocationService.RevokeDeviceTokensAsync(user.Id, "PNew Token Generated");

        return ApiResponse.Success(new
        {
            Message = "Session created successfully.",
            AccessToken = jwt,
            ExpiresIn = expiresIn,
            SessionId = sessionId
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error issuing session token");
        return ApiResponse.Error(ErrorHttp.Error, ex);
    }
}


    [HttpPost]
    [Route(RouteWebVersion1.Security.Auth.Logout)]
    public async Task<ApiResult> Logout()
    {
        try
        {
            return await Task.FromResult(ApiResponse.Success());
        }
        catch (Exception e)
        {
            logger.LogError("error");
            return await Task.FromResult(ApiResponse.Error(ErrorHttp.Error, e));
        }
    }

    [HttpPost]
    [Route(RouteWebVersion1.Security.Auth.UpgradeCheck)]
    public async Task<ApiResult> UpgradeCheck()
    {
        try
        {
            return await Task.FromResult(ApiResponse.Success());
        }
        catch (Exception e)
        {
            logger.LogError("error");
            return await Task.FromResult(ApiResponse.Error(ErrorHttp.Error, e));
        }
    }
    
    
}
