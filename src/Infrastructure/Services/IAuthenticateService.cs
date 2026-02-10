using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Bookazone.Api.Controllers.V1.Config.Request;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs;
using Bookazone.Application.DTOs.Converter;
using Bookazone.Application.Interfaces.Repository.Others;
using Bookazone.Application.Interfaces.Repository.Permission;
using Bookazone.Application.Interfaces.Repository.Profile;
using Bookazone.Application.Interfaces.Repository.Tenant;
using Bookazone.Domain.Entities.Profile;
using Bookazone.Domain.Entities.Tenant;
using Bookazone.Infrastructure.Services.JWT;
using Functions = Bookazone.Application.Common.Shared.Utils.Functions;

namespace Bookazone.Infrastructure.Services;

public interface IAuthenticateService
{
    dynamic UserData(Users user, Device device);
    dynamic GetProfile(Users user);

    void LogoutUser(Users user);
    public double CalculateDistance(double lat1, double lon1, double lat2, double lon2);
    public bool IsStrongPassword(string password);
    public string? GenerateToken(Users user, Device device);
    public ApiResult UsernameCheck(string? username, DeviceRequest deviceRequest, string? notificationToken = null);

    Task<(string Jwt, int ExpiresInSeconds, string SessionId)> GenerateJwtForUserAsync(Users user, Device device, Tenants tenant);

    public string GenerateVerificationToken(Users user, Device device);
    public (string Jwt, int ExpiresIn, Guid SessionId) GenerateSessionToken(Users user, Device device);
    public string GenerateJwt(IEnumerable<Claim> claims, int expiresInMinutes = 60);
    bool VerifyTokenHash(string plainToken, string hashedToken);
    string HashToken(string token);
    public IDictionary<string, object>? DecodeJwt(string token);
}

public class AuthenticateService(
    ILogger<AuthenticateService> logger,
    IAppConfigRepository appConfigRepository,
    IOneTimePasswordRepository otpRepository,
    IUserPermissionRepository userPermissionRepository,
    IDeviceRepository deviceRepository,
    IUserRoleRepository userRoleRepository, 
    IRefreshTokenRepository refreshTokenRepository,
    IUserRepository userRepository,
    IOptions<JwtOptions> options,
    ITenantRepository tenantRepository,
    IUserTenantRepository  userTenantRepository,
    IUserPermissionService userPermissionService)
    : IAuthenticateService
{
    private readonly JwtOptions _jwtOptions = options.Value;
    public static double ToRadians(double deg) => deg * Math.PI / 180;

    public dynamic UserData(Users user, Device device)
    {
        var customer = user.Id;
        return new
        {
            User = UserConverter.ToDto(user,  userPermissionRepository.FindByUserId(user.Id)),
        };
    }

    public dynamic GetProfile(Users user)
    {
        // Base DTO from converter
        var dto = UserConverter.ToDto(user, userPermissionRepository.FindByUserId(user.Id));

        // Fetch roles and permissions
        var roles = userRoleRepository.GetRolesByUserAsync(user.Id).Result
            .Where(r => r.Name != null)
            .Select(r => r.Name!)
            .Distinct()
            .ToList();

        
        var tenant = user.FkTenant != null ?  tenantRepository.GetByNameAsync(user.FkTenant.Name) : null;
        
        var memberships =  userTenantRepository.GetByUserIdAsync(user.Id).Result;

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
        
        
        var permissions = userPermissionService.GetUserPermissionNamesAsync(user.Id).Result
            .Distinct()
            .ToList();

        dto.Roles = roles;
        dto.Permissions = permissions;

        return new
        {
            Tenant = tenants,
            User = dto
        };
    }

    public void LogoutUser(Users user)
    {
        deviceRepository.ListByUser(user)?.ForEach(device =>
        {
            device.CurrentSessionToken = null;
            deviceRepository.Update(device);
        });
    }

public async Task<(string Jwt, int ExpiresInSeconds, string SessionId)> GenerateJwtForUserAsync(
    Users user,
    Device device,
    Tenants tenant)
{
    if (string.IsNullOrEmpty(_jwtOptions.SecretKey))
        return (string.Empty, 0, string.Empty);

    // 1️⃣ Create a new session
    var sessionId = Functions.GenerateUid(true);

    // 2️⃣ Base claims
    var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(JwtRegisteredClaimNames.UniqueName, user.Username ?? string.Empty),
        new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
        new Claim("device_id", device.Id.ToString()),
        new Claim("session_id", sessionId),
        new Claim("app_type", "web"),
        new Claim("fullname", $"{user.Firstname} {user.Lastname}".Trim())
    };

    // Add tenant claims only if tenant is provided
    if (tenant != null)
    {
        claims.Add(new Claim("tenant_id", tenant.Id.ToString() ?? string.Empty));
        claims.Add(new Claim("tenant_subdomain", tenant.Subdomain ?? string.Empty));
        claims.Add(new Claim("tenant_code", tenant.Code ?? string.Empty));
    }
    else
    {
        // Optional: You can add default values or skip the tenant claims
        claims.Add(new Claim("tenant_id", "no-tenant"));
        claims.Add(new Claim("tenant_subdomain", string.Empty));
        claims.Add(new Claim("tenant_code", string.Empty));
    }

    // 3️⃣ Add role & permission claims
    var roles = userRoleRepository.FindByUserId(user.Id)
        .Where(r => r.FkRole?.Name != null)
        .Select(r => r.FkRole!.Name)
        .Distinct()
        .ToList();

    foreach (var role in roles)
        claims.Add(new Claim(ClaimTypes.Role, role));

    var permissions = userPermissionRepository.FindByUserId(user.Id)
        .Where(p => p.FkPermission?.Name != null)
        .Select(p => p.FkPermission!.Name)
        .Distinct()
        .ToList();

    foreach (var perm in permissions)
        claims.Add(new Claim("permission", perm));

    // 4️⃣ Create signing credentials
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    var expires = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiryMinutes ?? 600);

    // 5️⃣ Generate JWT token
    var jwtToken = new JwtSecurityToken(
        issuer: _jwtOptions.Issuer,
        audience: _jwtOptions.Audience,
        claims: claims,
        notBefore: DateTime.UtcNow,
        expires: expires,
        signingCredentials: creds
    );

    var tokenString = new JwtSecurityTokenHandler().WriteToken(jwtToken);
    var newRefreshToken = await refreshTokenRepository.CreateAsync(user.Id, device.Id);
    await refreshTokenRepository.CleanUpOldTokensAsync(user.Id, device.Id);

    // 7️⃣ Update device session info
    device.CurrentSessionToken = CryptoService.Encrypt(tokenString);
    device.MemorySlug = sessionId;
    device.LastConnect = DateTime.UtcNow;
    deviceRepository.Update(device);

    // 8️⃣ Update user info
    user.LastAuthenticated = DateTime.UtcNow;
    user.TryCount = 0;
    user.LastTryCount = null;
    userRepository.Update(user);

    return (tokenString, (int)(expires - DateTime.UtcNow).TotalSeconds, sessionId);
}

    
    
    

    public string? GenerateToken(Users user, Device device)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString("N")),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username ?? string.Empty),
            new Claim("email", user.Email ?? ""),
            new Claim("tenant_id", user.FkTenant?.Id.ToString() ?? ""),
            new Claim("tenant_subdomain", user.FkTenant?.Subdomain ?? ""),
            new Claim("device_id", device.Id.ToString()),
            new Claim("session_id", Guid.NewGuid().ToString()),
            new Claim("app_type", "otp-verification"),
            new Claim("device_verified", "true"),
            new Claim("fullname", $"{user.Firstname} {user.Lastname}".Trim()),
            new(JwtRegisteredClaimNames.Name, user.Username ?? string.Empty),
            new(JwtRegisteredClaimNames.NameId, user.Username ?? string.Empty),
            new(JwtRegisteredClaimNames.GivenName, user.Username ?? string.Empty),
        };

        var perms = userPermissionRepository.FindByUserId(user.Id);
        perms.ForEach(userPerm =>
        {
            if (userPerm.FkPermission?.Name != null)
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

    public bool IsStrongPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            return false;

        bool hasUpper = password.Any(char.IsUpper);
        bool hasLower = password.Any(char.IsLower);
        bool hasDigit = password.Any(char.IsDigit);
        bool hasSymbol = password.Any(ch => !char.IsLetterOrDigit(ch));

        return hasUpper && hasLower && hasDigit && hasSymbol;
    }

    public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Earth radius in km
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c; // km
    }
    
    public ApiResult UsernameCheck(string? username, DeviceRequest deviceRequest, string? notificationToken = null)
    {
        var users = userRepository.FindByEmailOrPhoneOrUsername(username);
      //  var appStatus = appConfigRepository.FindBySlug(Consts.AppConfig.AppStatusKeyV2);

        if (users == null || !users.Any())
        {
            return ApiResponse.Error(ErrorHttp.UserNotFound);
        }

        /*if (
            appStatus?.Value != Consts.AppStatus.Active
            && users.First().AppStatusException != true
        )
            return ApiResponse.Error(ErrorHttp.AppInactive);*/

        var user = users.First();
        user.Username = new EmailAddressAttribute().IsValid(username)
            ? username
            : Functions.PhoneLocalFormat(username);
        userRepository.Update(user);
        logger.LogInformation("This is for username check");
        if (user == null)
        {
            var checkCustomerEmail = userRepository
                .FindByEmailOrPhoneOrUsername(user.Email)
                ?.FirstOrDefault();
            var checkCustomerPhone = userRepository
                .FindByEmailOrPhoneOrUsername(Functions.PhoneLocalFormat(user.Phone))
                ?.FirstOrDefault();
            if (
                checkCustomerEmail != null
                && checkCustomerPhone != null
                && checkCustomerEmail.Id != checkCustomerPhone.Id
            )
                return ApiResponse.Error(ErrorHttp.PhoneEmailExistingForDiffCustomer);
            var customer = checkCustomerPhone ?? checkCustomerEmail;
            if (customer == null)
            {
                customer = new Users
                {
                    Firstname = user.Firstname,
                    Lastname = user.Lastname,
                    Email = user.Email,
                    Phone = Functions.PhoneLocalFormat(user.Phone),
                    Address = user.Address,
                    Gender = user.Gender,
                    Birthdate = Convert.ToDateTime(user.Birthdate, CultureInfo.InvariantCulture),
                    PhoneVerified = user.PhoneVerified,
                    EmailVerified = user.EmailVerified,
                };
                userRepository.CreateAsync(customer);
            }

            user = customer;
            userRepository.Update(user);
        }

        logger.LogInformation("This is for username check");
        var devices = deviceRepository.ListByIdentifierAndUser(deviceRequest.Identifier, user);
        var device = devices?.Count >= 1
            ? devices[0]
            : deviceRepository.Create(
                new Device
                {
                    Slug = Functions.GenerateUid(),
                    Name = deviceRequest.Name,
                    Identifier = deviceRequest.Identifier,
                    Version = deviceRequest.Version,
                    System = deviceRequest.System,
                    Os = deviceRequest.Os,
                    AppVersion = deviceRequest.AppVersion,
                    NotificationToken = notificationToken,
                    FkUser = user,
                    Verified = user.Disable2Fa == true,
                    DateVerified = user.Disable2Fa == true ? DateTime.UtcNow : null,
                }
            );
        if (device == null)
            return ApiResponse.Error(ErrorHttp.DeviceVerifyNotAvailable);

        if (
            (user.Locked == true || user.TryCount >= 3)
            && user.LastTryCount != null
            && user.LastTryCount.Value > DateTime.UtcNow.AddMinutes(-15)
        )
            return ApiResponse.Error(ErrorHttp.UserLocked);
        if (user.Disable2Fa == true)
            return ApiResponse.Success(new { Message = "Ready for authenticate", user.Username });

        if (device.Verified == true && (user.EmailVerified == true || user.PhoneVerified == true))
        {
            return ApiResponse.Success(new { Message = "Ready for authenticate", user.Username });
        }

        logger.LogInformation("This is for username check");
        BookazoneOtpResponse? otpResponse;
        if (user.Email != null)
        {
            otpResponse = otpRepository
                .GenerateOtp(
                    new BookazoneOtpGenerate { Email = user.Email, Purpose = device.Name }
                )
                .Result;
            if (otpResponse == null)
                return ApiResponse.Error(ErrorHttp.DeviceVerifyNotAvailable);
            device.OtpEmailReference = otpResponse.Reference;
        }

        if (user.Phone == null)
            return ApiResponse.OtpRequired(
                new { DeviceVerified = false, Token = GenerateToken(user, device) }
            );
        otpResponse = otpRepository
            .GenerateOtp(new BookazoneOtpGenerate { Phone = user.Phone, Purpose = device.Name })
            .Result;
        if (otpResponse == null)
            return ApiResponse.Error(ErrorHttp.DeviceVerifyNotAvailable);
        device.OtpPhoneReference = otpResponse.Reference;
        return ApiResponse.OtpRequired(
            new { DeviceVerified = false, Token = GenerateToken(user, device) }
        );
    }
    
    
    public string GenerateVerificationToken(Users user, Device device)
{
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim("device_id", device.Id.ToString()),
        new Claim("purpose", "verification"),
        new Claim("device_verified", "true")
    };

    var token = new JwtSecurityToken(
        issuer: _jwtOptions.Issuer,
        audience: _jwtOptions.Audience,
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(30),
        signingCredentials: creds
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}

public (string Jwt, int ExpiresIn, Guid SessionId) GenerateSessionToken(Users user, Device device)
{
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var sessionId = Guid.NewGuid();

    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim("tenant_id", user.FkTenant.ToString()),
        new Claim("device_id", device.Id.ToString()),
        new Claim("session_id", sessionId.ToString()),
        new Claim("app_type", "web"),
        new Claim("device_verified", "true"),
        new Claim("fullname", user.Firstname + " " + user.Lastname ?? "")
    };

    // Add roles if needed
    var expires = DateTime.UtcNow.AddHours(4);

    var token = new JwtSecurityToken(
        issuer: _jwtOptions.Issuer,
        audience: _jwtOptions.Audience,
        claims: claims,
        expires: expires,
        signingCredentials: creds
    );

    return (new JwtSecurityTokenHandler().WriteToken(token), (int)(expires - DateTime.UtcNow).TotalSeconds, sessionId);
}

    
public string GenerateJwt(IEnumerable<Claim> claims, int expiresInMinutes = 60)
{
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var jwt = new JwtSecurityToken(
        issuer: _jwtOptions.Issuer ?? "BookazoneAuthServer",
        audience: _jwtOptions.Audience ?? "BookazoneClients",
        claims: claims, // ✅ ensures tenant_id and all others are included
        notBefore: DateTime.UtcNow,
        expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
        signingCredentials: creds
    );

    return new JwtSecurityTokenHandler().WriteToken(jwt);
}


public IDictionary<string, object>? DecodeJwt(string token)
{
    if (string.IsNullOrWhiteSpace(token))
        return null;

    var handler = new JwtSecurityTokenHandler();
    var jwt = handler.ReadJwtToken(token);

    return jwt.Claims.ToDictionary(c => c.Type, c => (object)c.Value);
} 
    
public string HashToken(string token)
{
    using var sha = SHA256.Create();
    var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(token));
    return Convert.ToBase64String(bytes);
}

public bool VerifyTokenHash(string plainToken, string hashedToken)
{
    var computedHash = HashToken(plainToken);
    return computedHash == hashedToken;
} 
    
}