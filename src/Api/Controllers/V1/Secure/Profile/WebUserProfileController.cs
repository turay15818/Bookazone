using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Bookazone.Api.Controllers.V1.Config;
using Bookazone.Api.Controllers.V1.Config.Request;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs;
using Bookazone.Application.DTOs.Converter;
using Bookazone.Application.Interfaces.Repository.Permission;
using Bookazone.Application.Interfaces.Repository.Profile;
using Bookazone.Application.Interfaces.Repository.Security;
using Bookazone.Domain.Entities.Profile;
using Bookazone.Infrastructure.Services;
using Bookazone.Infrastructure.Services.Files;
using Bookazone.Infrastructure.Services.JWT;
using Functions = Bookazone.Application.Common.Shared.Utils.Functions;

namespace Bookazone.Api.Controllers.V1.Secure.Profile;

// [Authorize]
[ApiController]
[ApiExplorerSettings(GroupName = SwaggerConfig.SwaggerDocName.WebVersion1.Slug)]
[Route(RouteWebVersion1.Secure.User.Base)]
public class WebUserProfileController(
    IDeviceRepository deviceRepository,
    IUserRepository userRepository,
    ILogger<WebUserProfileController> logger,
    Functions functions,
    IEmailService emailService,
    IUserAccessService userAccessService,
    TokenRevocationService tokenRevocationService,
    IUserPermissionRepository userPermissionRepository,
    IAuthenticateService authenticateService,
    IOneTimePasswordRepository otpRepository,
    IUserPasswordRepository userPasswordRepository,
    IUserPermissionService userPermissionService,
    IOptions<JwtOptions> options
) : ControllerBase
{
    private readonly JwtOptions _jwtOptions = options.Value;

    [HttpGet]
    [Route(RouteWebVersion1.Secure.User.Profile)]
    public async Task<ApiResult> Profile()
    {
        try
        {
            var user = functions.GetUser(User.Identity?.Name);
            logger.LogError("User Information: {User}", user);
            if (user == null)
                return await Task.FromResult(ApiResponse.Error(ErrorHttp.TokenExpired));

            return await Task.FromResult(ApiResponse.Success(authenticateService.GetProfile(user)));
        }
        catch (Exception e)
        {
            logger.LogError("error");
            return await Task.FromResult(ApiResponse.Error(ErrorHttp.Error, e));
        }
    }

    [HttpPost]
    [Route(RouteWebVersion1.Secure.User.UserExists)]
    public async Task<ApiResult> CheckIfUserExists([FromBody] CheckUserExistsRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.username))
                return await Task.FromResult(ApiResponse.Error(ErrorHttp.FormInvalid));

            var exists = userRepository.ExistsByEmailOrPhoneOrUsername(request.username);

            return await Task.FromResult(ApiResponse.Success(new { exists }));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error checking user existence.");
            return await Task.FromResult(ApiResponse.Error(ErrorHttp.Error, e));
        }
    }

    [HttpGet]
    [Authorize(Roles = Consts.Permissions.TenantView)]
    [Route(RouteWebVersion1.Secure.User.All)]
    public async Task<ApiResult> All(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? q = null,
        [FromQuery] Guid? shopId = null
    )
    {
        try
        {
            var user = functions.GetUser(User.Identity?.Name);
            if (user == null)
                return await Task.FromResult(ApiResponse.Error(ErrorHttp.TokenExpired));
            var result = userRepository.AllByTenantDto(user.FkTenant!.Id, page, pageSize, q, shopId);
            return await Task.FromResult(
                ApiResponse.Success(result.Items, pages: result.Pages, pageIndex: page)
            );
        }
        catch (Exception e)
        {
            logger.LogError("error");
            return await Task.FromResult(ApiResponse.Error(ErrorHttp.Error, e));
        }
    }

    [HttpGet]
    [Route(RouteWebVersion1.Secure.User.Find)]
    public async Task<ApiResult> Find(Guid id)
    {
        try
        {
            var user = functions.GetUser(User.Identity?.Name);
            if (user == null)
                return await Task.FromResult(ApiResponse.Error(ErrorHttp.TokenExpired));

            var userTier = userRepository.Find(id);
            return await Task.FromResult(
                ApiResponse.Success(userTier != null ? UserConverter.ToDto(userTier) : null)
            );
        }
        catch (Exception e)
        {
            logger.LogError("error");
            return await Task.FromResult(ApiResponse.Error(ErrorHttp.Error, e));
        }
    }

    [HttpGet]
    [Route(RouteWebVersion1.Secure.User.FindByShopId)]
    public async Task<ApiResult> FindByShop(Guid fkShopId)
    {
        try
        {
            var user = functions.GetUser(User.Identity?.Name);
            if (user == null)
                return await Task.FromResult(ApiResponse.Error(ErrorHttp.TokenExpired));
            var userTier = userRepository.FindByShop(fkShopId);
            return await Task.FromResult(
                ApiResponse.Success(userTier != null ? UserConverter.ToDto(userTier) : null)
            );
        }
        catch (Exception e)
        {
            logger.LogError("error");
            return await Task.FromResult(ApiResponse.Error(ErrorHttp.Error, e));
        }
    }

    
    
    [HttpPost]
    [Authorize(Roles = Consts.Permissions.TenantUpdate)]
    [Route(RouteWebVersion1.Secure.User.Delete)]
    public async Task<ApiResult> Delete(Guid id)
    {
        try
        {
            var user = functions.GetUser(User.Identity?.Name);
            if (user == null)
                return await Task.FromResult(ApiResponse.Error(ErrorHttp.TokenExpired));
            if (user == null)
                return await Task.FromResult(ApiResponse.Error(ErrorHttp.CustomerNotFound));
            var userTier = userRepository.Find(id);
            if (userTier == null)
                return await Task.FromResult(ApiResponse.Error(ErrorHttp.UserNotFound));
            if (user.Id == userTier.Id)
                return await Task.FromResult(ApiResponse.Error(ErrorHttp.SelfActionNotAllow));
            userRepository.Delete(userTier.Id);
            return await Task.FromResult(ApiResponse.Success());
        }
        catch (Exception e)
        {
            logger.LogError("error");
            return await Task.FromResult(ApiResponse.Error(ErrorHttp.Error, e));
        }
    }

    [HttpPost]
    [Route(RouteWebVersion1.Secure.User.Create)]
    public async Task<ApiResult> CreateUser([FromBody] UserRequest userRequest)
    {
        try
        {
            userRequest.Phone = Functions.PhoneLocalFormat(userRequest.Phone);
            if (userRepository.FindByEmailOrPhoneOrUsername(userRequest.Email)?.Count > 0)
                return ApiResponse.Error(ErrorHttp.EmailExisting);
            if (userRepository.FindByEmailOrPhoneOrUsername(userRequest.Email)?.Count > 0)
                return ApiResponse.Error(ErrorHttp.EmailExisting);
            if (userRepository.FindByEmailOrPhoneOrUsername(userRequest.Phone)?.Count > 0)
                return ApiResponse.Error(ErrorHttp.PhoneExisting);
            string? profileImageUrl = null;
            
            if (!authenticateService.IsStrongPassword(userRequest?.Password)) 
                return ApiResponse.Error(ErrorHttp.WeakPassword, new Exception("Weak password."));
            

            var newUser = new Users
            {
                Email = userRequest.Email,
                Phone = userRequest.Phone,
                Firstname = userRequest.Firstname,
                Lastname = userRequest.Lastname,
                Address = userRequest.Address,
                Username = userRequest.Email ?? userRequest.Phone,
                PasswordExpired = false,
                ProfileImage = profileImageUrl,
                Gender = userRequest.Gender,
                Birthdate = userRequest.Birthdate,
                CreatedBy = userRequest.Email,
                DateCreated = DateTime.UtcNow,
            };
            newUser = await userRepository.CreateAsync(newUser)!;
            if (newUser == null)
            {
                logger.LogError("Failed to create user in database");
                return ApiResponse.Error(ErrorHttp.DbStatusError);
            }

            logger.LogInformation("User created successfully with ID: {UserId}", newUser.Id);
            

            var otp = otpRepository.Generate(
                userRequest.Email,
                userRequest.Phone,
                "Create User",
                userRequest.DeviceRequest
            );
            
            
          
                    await userPasswordRepository.CreatePasswordAsync(newUser.Id, userRequest.Password, newUser.Username);
                    await userPermissionService.SetupCustomerAsync(newUser, newUser.Username ?? userRequest.Email ?? "system");
                    await userPasswordRepository.CreatePasswordAsync(newUser.Id, userRequest.Password, newUser.Email ?? "system");


            var device = new Device
            {
                Name = userRequest.DeviceRequest.Name,
                Identifier = userRequest.DeviceRequest.Identifier,
                Version = userRequest.DeviceRequest.Version,
                AppVersion = userRequest.DeviceRequest.AppVersion,
                System = userRequest.DeviceRequest.System,
                Os = userRequest.DeviceRequest.Os,
                OtpEmailReference = otp.Reference,
                OtpPhoneReference = otp.Reference,
                Verified = false,
                CreatedBy = newUser.Username,
                DateCreated = DateTime.UtcNow,
                LastConnect = DateTime.UtcNow,
                FkUser = newUser,
            };

            var savedDevice = deviceRepository.Create(device);
            if (savedDevice != null)
            {
                var token = GenerateToken(newUser, savedDevice);
                if (token == null) return ApiResponse.Error(ErrorHttp.TokenExpired);
              
               return ApiResponse.Success(new { Token = token, Users = UserConverter.ToDto(newUser) });
               // return ApiResponse.Success(new {Users = UserConverter.ToDto(newUser) });
            }

            logger.LogError("Device creation failed for user {UserId}", newUser.Id);
            return ApiResponse.Error(ErrorHttp.DbStatusError);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating user");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
    

[HttpPost]
[Authorize(Roles = Consts.Permissions.TenantUsersInvite)]
[Route(RouteWebVersion1.Secure.User.Invite)]
public async Task<ApiResult> InviteUser([FromBody] UserInviteRequest request)
{
    await using var transaction = await userRepository.BeginTransactionAsync();
    try
    {
        var inviter = functions.GetUser(User.Identity?.Name);
        if (inviter == null) return ApiResponse.Error(ErrorHttp.TokenExpired);
        var tenant = inviter.FkTenant;
        if (tenant == null) return ApiResponse.Error(ErrorHttp.Unauthorized, new Exception("No tenant associated with this user."));
        var existingUser = userRepository.FindByEmailOrPhoneOrUsername(request.Email)?.FirstOrDefault();
        if (existingUser != null) return ApiResponse.Error(ErrorHttp.AlreadyExists, new Exception("A user with this email already exists."));
        var invitedUser = new Users
        {
            Id = Guid.NewGuid(),
            Firstname = request.FirstName,
            Lastname = request.LastName,
            Email = request.Email,
            Username = request.Email.Split('@')[0],
            Active = false,
            Deleted = false,
            Frozen = false,
            FkTenant = tenant,
            CreatedBy = inviter.Username,
            DateCreated = DateTime.UtcNow,
            EmailVerified = false
        };
        await userRepository.CreateAsync(invitedUser);
        if (inviter.FkTenant != null)
        {
            var tenantId = inviter.FkTenantId; 
            if (tenantId == null)
            {
                var inviterUser = userRepository.Find(inviter.Id);
                tenantId = inviterUser?.FkTenantId;
            }

            var claims = new List<Claim>
            {
                new Claim("purpose", "User Invitation"),
                new Claim("user_id", invitedUser.Id.ToString()),
                new Claim("email", invitedUser.Email ?? ""),
                new Claim("tenant_id", tenantId?.ToString() ?? string.Empty)
            };
            logger.LogInformation("Inviting user with claims: {@Claims}", claims.Select(c => new { c.Type, c.Value }));

            var inviteToken = authenticateService.GenerateJwt(claims, expiresInMinutes: 60 * 24);
            var tokenString = inviteToken ?? string.Empty;
            var otpResponse = await otpRepository.GenerateOtp(new BookazoneOtpGenerate
            {
                Email = invitedUser.Email,
                Purpose = "User Invitation",
                IsTokenBased = true,
                ExtraClaims = new Dictionary<string, string>
                {
                    { "tenant_id", tenant.Id.ToString() }
                }
            });
            await userAccessService.CreateOrUpdateUserWithAccessAsync(
                invitedUser,
                request.Roles,
                request.Permissions,
                inviter.Username
            );
            var inviteLink = !string.IsNullOrEmpty(request.RedirectUrl)
                ? $"{request.RedirectUrl}?token={tokenString}"
                : $"https://app.Bookazone.com/invite-accept?token={tokenString}";

            var emailBody = emailService.GenerateUserInviteEmailHtml(
                invitedUser.Firstname ?? "User",
                inviter.Firstname ?? "Admin",
                tenant.Name,
                inviteLink
            );

            if (!string.IsNullOrEmpty(invitedUser.Email))
            {
                await emailService.SendEmailAsync(
                    invitedUser.Email,
                    $"You're invited to join {tenant.Name}",
                    emailBody
                );

                await transaction.CommitAsync();

                return ApiResponse.Success(new
                {
                    message = $"Invitation sent to {invitedUser.Email}.",
                    invite_token = tokenString,
                    expires_in = 86400
                });
            }
        }

        return ApiResponse.Error(ErrorHttp.BadRequest, new Exception("Email not provided."));

    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        logger.LogError(ex, "Error sending user invitation");
        return ApiResponse.Error(ErrorHttp.Error, ex);
    }
}

    
[HttpPost]
//[Authorize(Roles = Consts.Permissions.TenantUsersManage)]
[Route(RouteWebVersion1.Secure.User.Update)]
public async Task<ApiResult> Update([FromBody] UserDto userRequest)
{
    await using var transaction = await userRepository.BeginTransactionAsync();

    try
    {
        var authUser = functions.GetUser(User.Identity?.Name);
        if (authUser == null) return ApiResponse.Error(ErrorHttp.TokenExpired);
        var userToUpdate = userRepository.Find(userRequest.Id);
        if (userToUpdate == null) return ApiResponse.Error(ErrorHttp.UserNotFound);
        userToUpdate.Firstname = userRequest.FirstName ?? userToUpdate.Firstname;
        userToUpdate.Lastname = userRequest.LastName ?? userToUpdate.Lastname;
        userToUpdate.Username = userRequest.Username ?? userToUpdate.Username;
        userToUpdate.Email = userRequest.Email ?? userToUpdate.Email;
        userToUpdate.Phone = Functions.PhoneLocalFormat(userRequest.Phone) ?? userToUpdate.Phone;
        userToUpdate.Address = userRequest.Address ?? userToUpdate.Address;
        userToUpdate.Birthdate = userRequest.Birthdate ?? userToUpdate.Birthdate;
        userToUpdate.Frozen = userRequest.Frozen;
        userRepository.Update(userToUpdate);
        var (_, rolesChanged, permsChanged) = await userAccessService.CreateOrUpdateUserWithAccessAsync(
            userToUpdate,
            userRequest.Roles,
            userRequest.Permissions,
            authUser.Username
        );
        await transaction.CommitAsync();
        if (rolesChanged || permsChanged)
            authenticateService.LogoutUser(userToUpdate);
        return ApiResponse.Success(UserConverter.ToDto(userToUpdate));
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        logger.LogError(ex, "Error updating user {UserId}", userRequest.Id);
        return ApiResponse.Error(ErrorHttp.Error, ex);
    }
}

    
    [HttpPost]
   // [Authorize(Roles = Consts.Permissions.TenantUsersManage)]
    [Route(RouteWebVersion1.Secure.User.UpdatePassword)]
    public async Task<ApiResult> UpdatePassword(PasswordResetConfirmRequest userRequest)
    {
            
            await using var transaction = await otpRepository.BeginTransactionAsync();
            
                try
                {
                    var user = functions.GetUser(User.Identity?.Name);
                    if (user == null)
                        return await Task.FromResult(ApiResponse.Error(ErrorHttp.TokenExpired));
                    var userTier = userRepository.Find(user.Id);
                    if (userTier == null)
                        return await Task.FromResult(ApiResponse.Error(ErrorHttp.UserNotFound));
                    if (userRequest.NewPassword != userRequest.PasswordRepeat)
                    {
                        return await Task.FromResult(ApiResponse.Error(ErrorHttp.PasswordNotMatch));
                    }

                    var device = await deviceRepository.GetByIdentifierAsync(userRequest.DeviceRequest.Identifier, user.Id);
                    
                    if (!authenticateService.IsStrongPassword(userRequest.NewPassword))
                        return ApiResponse.Error(ErrorHttp.WeakPassword, new Exception(
                            "Password must be at least 8 characters long, contain uppercase, lowercase, number, and special character."
                        ));
            
                    
                    var passwordValid = await userPasswordRepository.ValidatePasswordAsync(user.Id, userRequest.OldPassword);
                    if (!passwordValid)
                    {
                        user.TryCount++;
                        user.LastTryCount = DateTime.UtcNow;
                        userRepository.Update(user);
                        return ApiResponse.Error(ErrorHttp.OldPasswordNotMatching);
                    }
                    
                    // 6️⃣ Check password reuse
                    var reused = await userPasswordRepository.IsPasswordReusedAsync(user.Id, userRequest.NewPassword);
                    if (reused)
                        return ApiResponse.Error(ErrorHttp.PasswordReused, new Exception("You cannot reuse a previous password."));
            
                    // 7️⃣ Set new password
                    await userPasswordRepository.CreatePasswordAsync(user.Id, userRequest.NewPassword, user.Email ?? "system");
            
                    // 8️⃣ Unlock account if necessary
                    user.Locked = false;
                    user.TryCount = 0;
                    user.LastTryCount = null;
                    userRepository.Update(user);
            
                    await transaction.CommitAsync();
                    await tokenRevocationService.RevokeAllUserTokensAsync(user.Id, "Password changed");
                    logger.LogInformation("Password reset successfully for user {UserId}", user.Id);
            
                    if (string.IsNullOrEmpty(userRequest.DeviceRequest.Identifier))
                        throw new Except(ErrorHttp.BadRequest);
                    userTier.UpdatedBy = userTier.Firstname + " " + userTier.Lastname;
                    userTier.DateUpdated = DateTime.UtcNow;
                    userTier = userRepository.Update(userTier);
                    if (userTier != null)
                        authenticateService.LogoutUser(userTier);
                    
                    
                    return ApiResponse.Success(new
                    {
                        message = "Password reset successfully. You can now log in."
                    });
                    
        }
        catch (Exception e)
        {
            logger.LogError("error");
            return await Task.FromResult(ApiResponse.Error(ErrorHttp.Error, e));
        }
    }
    
    private string? GenerateToken(Users user, Device device)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString("N")),
            new(JwtRegisteredClaimNames.UniqueName, user.Username ?? string.Empty),
            new(JwtRegisteredClaimNames.Name, user.Username ?? string.Empty),
            new(JwtRegisteredClaimNames.NameId, user.Username ?? string.Empty),
            new(JwtRegisteredClaimNames.GivenName, user.Username ?? string.Empty),
        };

        var perms = userPermissionRepository.FindByUserId(user.Id);

        perms.ForEach(userPerm =>
        {
            if (userPerm.Active && !userPerm.Deleted && userPerm.FkPermission?.Name != null)
                claims.Add(new Claim(ClaimTypes.Role, userPerm.FkPermission.Name));
        });

        if (_jwtOptions.SecretKey == null)
            return null;

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey)),
            SecurityAlgorithms.HmacSha256Signature
        );

        var token = new JwtSecurityToken(
            _jwtOptions.Issuer,
            _jwtOptions.Audience,
            claims,
            expires: DateTime.UtcNow.AddDays(30),
            signingCredentials: signingCredentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        user.TryCount = 0;
        user.LastTryCount = null;
        user.LastAuthenticated = DateTime.UtcNow;
        userRepository.Update(user);

        device.FkUser = user;
        device.LastConnect = DateTime.UtcNow;
        device.CurrentSessionToken = CryptoService.Encrypt(tokenString ?? string.Empty);
        deviceRepository.Update(device);

        return tokenString;
    }
}

public class CheckUserExistsRequest
{
    public string? username { get; set; } = string.Empty;
}
