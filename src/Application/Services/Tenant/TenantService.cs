using System.Security.Cryptography;
using Bookazone.Api.Controllers.V1.Config.Request;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Converter;
using Bookazone.Application.Interfaces.Context;
using Bookazone.Application.Interfaces.Repository.Profile;
using Bookazone.Application.Interfaces.Repository.Tenant;
using Bookazone.Application.Interfaces.Services;
using Bookazone.Domain.Entities.Profile;
using Bookazone.Domain.Entities.Tenant;
using Bookazone.Infrastructure.Persistence;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Services;
using Bookazone.Infrastructure.Services.Files;
using Functions = Bookazone.Application.Common.Shared.Utils.Functions;

namespace Bookazone.Application.Services.Tenant;

public class TenantService(
    ITenantRepository tenantRepo,
    IUserRepository userRepo,
    IDeviceRepository deviceRepo,
    IOneTimePasswordRepository otpRepo,
    IUserPasswordRepository userPasswordRepo,
    IUserPermissionService userPermissionService,
    IAuthenticateService authenticateService,
    IEmailService emailService,
    IUserContext userContext,
    IHttpContextAccessor httpContext,
    ILogger<TenantService> logger,
    BookazoneDbContext db
) : ITenantService
{
    public async Task<ApiResult> GetAllAsync(string? q, int pageSize, int pageIndex)
    {
        var query = tenantRepo.Query().Where(t => !t.Deleted && t.Active);
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(SearchExpressionBuilder.BuildSearchExpression<Tenants>(q));

        var paged = await query.ToPagedListAsync(pageIndex, pageSize);
        var dto = paged.Items.Select(TenantConverter.ToDto);
        return ApiResponse.Success(dto, pages: paged.TotalPages, pageIndex: pageIndex);
    }

    public async Task<ApiResult> GetByIdAsync(Guid id)
    {
        var tenant = await tenantRepo.GetByIdAsync(id);
        return tenant == null
            ? ApiResponse.Error(ErrorHttp.NotFound)
            : ApiResponse.Success(TenantConverter.ToDto(tenant));
    }

    public async Task<ApiResult> CreateAsync(TenantRequest request)
    {
        var httpRequest = httpContext.HttpContext!.Request;

        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            var existingTenant = await tenantRepo.GetByNameAsync(request.Name);
            if (existingTenant != null)
                return ApiResponse.Error(
                    ErrorHttp.AlreadyExists,
                    new Exception("Tenant already exists")
                );

            // Upload logo
            /*string? logoUrl = null;
            if (request.Logo != null)
                logoUrl = await firebaseStorageService.UploadFileAsync(
                    request.Logo,
                    "Tenants-Logo"
                );*/

            // Create Tenant
            var tenant = new Tenants
            {
                Name = request.Name,
                Description = request.Description,
                Email = request.Email,
                Phone = request.Phone,
                Address = request.Address,
                City = request.City,
                State = request.State,
                Country = request.Country,
                ZipCode = request.ZipCode,
               // Logo = logoUrl,
                IsVerified = false,
                Active = true,
                Deleted = false,
                CreatedBy = request.AdminUser?.Email ?? "system",
            };
            await tenantRepo.AddAsync(tenant);

            // Create Admin User
            var admin = new Users
            {
                Firstname = request.AdminUser?.Firstname ?? "Admin",
                Lastname = request.AdminUser?.Lastname ?? "User",
                Email = request.AdminUser?.Email,
                Phone = request.AdminUser?.Phone,
                Username = request.AdminUser?.Email ?? request.AdminUser?.Phone,
                FkTenant = tenant,
                Active = true,
                EmailVerified = false,
                PhoneVerified = false,
                Locked = false,
                CreatedBy = request.AdminUser?.Email ?? "system",
                DateCreated = DateTime.UtcNow,
            };
            await userRepo.CreateAsync(admin)!;

            // Create Device
            var device = new Device
            {
                FkUser = admin,
                Identifier = request.DeviceRequest!.Identifier,
                Name = request.DeviceRequest!.Name,
                Os = request.DeviceRequest!.Os,
                Version = request.DeviceRequest!.Version,
                AppVersion = request.DeviceRequest!.AppVersion,
                Slug = Functions.GenerateUid(true),
                Verified = false,
                VerifyByEmail = false,
                Active = true,
                Deleted = false,
                CreatedBy = admin.Username ?? "system",
            };
            await deviceRepo.CreateAsync(device);

            // Assign Roles and Permissions
            await userPermissionService.SetupTenantAdminAsync(admin, tenant, admin.Username!);
            await userPermissionService.SetupUserWithRolesAsync(
                admin,
                [Consts.Roles.TenantAdmin],
                tenant,
                admin.Username!
            );
            await userPermissionService.InitializeDefaultTenantPermissionsAsync(
                tenant,
                admin.Username!
            );

            await userPermissionService.SetupCustomerAsync(admin, admin.Username ?? "system");
            // Create OTP
            var otp = new OneTimePassword
            {
                FkUser = admin,
                Email = admin.Email,
                Purpose = "Account Verification",
                Token = Guid.NewGuid().ToString("N") + Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                IsTokenBased = true,
                Used = false,
                TransmitEmail = true,
                ExpiredDate = DateTime.UtcNow.AddDays(2),
                DateCreated = DateTime.UtcNow,
            };
            otpRepo.Create(otp);

            await db.SaveChangesAsync();
            await tx.CommitAsync();

            // Send Verification Email
            var link =
                $"{httpRequest.Scheme}://{httpRequest.Host}/api/v1/auth/verify?token={otp.Token}";
            var body = emailService.GenerateVerificationEmailHtml(admin.Firstname ?? "User", link);
            await emailService.SendEmailAsync(admin.Email!, "Verify Your Bookazone Account", body);

            return ApiResponse.Success(
                new
                {
                    tenant.Id,
                    tenant.Name,
                    admin.Email,
                }
            );
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            logger.LogError(ex, "Error creating tenant");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> VerifyAccountAsync(VerifyAccountRequest request)
    {
        await using var tx = await otpRepo.BeginTransactionAsync();
        try
        {
            if (
                string.IsNullOrWhiteSpace(request.Token)
                || string.IsNullOrWhiteSpace(request.Password)
            )
                return ApiResponse.Error(
                    ErrorHttp.BadRequest,
                    new Exception("Token and password required")
                );

            var otp = await otpRepo.GetByTokenAsync(request.Token);
            if (otp == null || otp.ExpiredDate < DateTime.UtcNow)
                return ApiResponse.Error(
                    ErrorHttp.TokenExpired,
                    new Exception("Token expired or invalid")
                );

            if (otp.Used)
                return ApiResponse.Error(ErrorHttp.Forbidden, new Exception("Token already used"));

            var user = otp.FkUser ?? throw new Exception("Associated user not found");
            var tenant = user.FkTenant ?? throw new Exception("Associated tenant not found");
            var device = await deviceRepo.GetByIdentifierAsync(request.Device.Identifier, user.Id);
            if (device == null)
            {
                device = new Device
                {
                    FkUser = user,
                    Identifier = request.Device.Identifier,
                    Name = request.Device.Name,
                    Os = request.Device.Os,
                    Version = request.Device.Version,
                    AppVersion = request.Device.AppVersion,
                    Slug = Functions.GenerateUid(true),
                    Verified = true,
                    VerifyByEmail = true,
                    Active = true,
                    Deleted = false,
                    CreatedBy = user.Username ?? "system",
                };
                await deviceRepo.CreateAsync(device);
            }

            otp.Used = true;
            otp.UsedTimeDate = DateTime.UtcNow;
            await otpRepo.UpdateAsync(otp);
            user.EmailVerified = true;
            user.Active = true;
            await userPasswordRepo.CreatePasswordAsync(
                user.Id,
                request.Password,
                user.Email ?? "system"
            );

            device.Verified = true;
            device.LastConnect = DateTime.UtcNow;
            await db.SaveChangesAsync();

            await tx.CommitAsync();
            var (jwt, expiresIn, sessionId) = await authenticateService.GenerateJwtForUserAsync(
                user,
                device,
                tenant
            );
            return ApiResponse.Success(
                new
                {
                    message = "Account verified successfully.",
                    token = jwt,
                    expires_in = expiresIn,
                    session_id = sessionId,
                    tenant = new
                    {
                        tenant.Id,
                        tenant.Name,
                        tenant.Email,
                    },
                    user = new
                    {
                        user.Id,
                        user.Firstname,
                        user.Lastname,
                        user.Email,
                    },
                }
            );
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            logger.LogError(ex, "Error verifying tenant account");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> UpdateAsync(TenantUpdateRequest request)
    {
        var currentUser = userContext.Username ?? "system";
        var user = (
            userRepo.FindByEmailOrPhoneOrUsername(currentUser)
            ?? throw new InvalidOperationException()
        ).FirstOrDefault();
        if (user?.FkTenant == null)
            return ApiResponse.Error(ErrorHttp.Unauthorized);

        var tenant = await tenantRepo.GetByIdAsync(user.FkTenant.Id);
        if (tenant == null)
            return ApiResponse.Error(ErrorHttp.NotFound);

        tenant.Name = request.Name;
        tenant.Description = request.Description ?? tenant.Description;
        tenant.Email = request.Email ?? tenant.Email;
        tenant.Phone = request.Phone ?? tenant.Phone;
        tenant.Address = request.Address ?? tenant.Address;
        tenant.City = request.City ?? tenant.City;
        tenant.State = request.State ?? tenant.State;
        tenant.Country = request.Country ?? tenant.Country;
        tenant.ZipCode = request.ZipCode ?? tenant.ZipCode;

        /*if (request.Logo != null)
        {
            var logoUrl = await firebaseStorageService.UploadFileAsync(
                request.Logo,
                "company-logos"
            );
            if (!string.IsNullOrEmpty(logoUrl))
                tenant.Logo = logoUrl;
        }*/

        await tenantRepo.UpdateAsync(tenant);
        return ApiResponse.Success(TenantConverter.ToDto(tenant));
    }

    public async Task<ApiResult> DeleteAsync(Guid id)
    {
        try
        {
            var deleted = await tenantRepo.DeleteAsync(id);
            return deleted
                ? ApiResponse.Success("Tenant deleted")
                : ApiResponse.Error(ErrorHttp.DbDeleteError);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting tenant {TenantId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> GetProfileAsync()
    {
        var userId = userContext.UserId;
        var tenantId = userContext.TenantId;
        logger.LogInformation("UserId: {UserId}, TenantId: {TenantId}", userId, tenantId);
        if (userId == null)
            return ApiResponse.Error(ErrorHttp.TokenExpired);
        if (tenantId == null)
            return ApiResponse.Error(
                ErrorHttp.BadRequest,
                new Exception("Tenant ID missing from token.")
            );
        var tenantDetails = await tenantRepo.GetDetailsAsync(tenantId.Value);
        return tenantDetails == null
            ? ApiResponse.Error(ErrorHttp.NotFound)
            : ApiResponse.Success(tenantDetails);
    }
}
