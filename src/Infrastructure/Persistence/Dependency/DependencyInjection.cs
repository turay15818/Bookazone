using System.IO;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Bookazone.Api.Middleware;
using Bookazone.Application.Common.Shared.Helpers;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs;
using Bookazone.Application.Interfaces.Context;
using Bookazone.Application.Interfaces.Repository.Others;
using Bookazone.Application.Interfaces.Repository.Booking;
using Bookazone.Application.Interfaces.Repository.Permission;
using Bookazone.Application.Interfaces.Repository.Profile;
using Bookazone.Application.Interfaces.Repository.Security;
using Bookazone.Application.Interfaces.Repository.Subscription;
using Bookazone.Application.Interfaces.Repository.Sports;
using Bookazone.Application.Interfaces.Repository.Tenant;
using Bookazone.Application.Interfaces.Repository.Events;
using Bookazone.Application.Interfaces.Repository.Equipment;
using Bookazone.Application.Interfaces.Repository.Vehicles;
using Bookazone.Application.Interfaces.Repository.Rentals;
using Bookazone.Application.Interfaces.Repository.Reviews;
using Bookazone.Application.Interfaces.Services.Booking;
using Bookazone.Application.Interfaces.Services;
using Bookazone.Application.Interfaces.Services.Others;
using Bookazone.Application.Interfaces.Services.Profile;
using Bookazone.Application.Interfaces.Services.Events;
using Bookazone.Application.Interfaces.Services.Equipment;
using Bookazone.Application.Interfaces.Services.Vehicles;
using Bookazone.Application.Interfaces.Services.Rentals;
using Bookazone.Application.Interfaces.Services.Reviews;
using Bookazone.Application.Interfaces.Services.Tenant;
using Bookazone.Application.Provider.Api;
using Bookazone.Application.Services.Tenant;
using Bookazone.Domain.Entities.Profile;
using Bookazone.Infrastructure.Authorization;
using Bookazone.Infrastructure.Identity;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Persistence.Repository.Others;
using Bookazone.Infrastructure.Persistence.Repository.Booking;
using Bookazone.Infrastructure.Persistence.Repository.Permission;
using Bookazone.Infrastructure.Persistence.Repository.Profile;
using Bookazone.Infrastructure.Persistence.Repository.Subscription;
using Bookazone.Infrastructure.Persistence.Repository.Sports;
using Bookazone.Infrastructure.Persistence.Repository.Tenant;
using Bookazone.Infrastructure.Persistence.Repository.Events;
using Bookazone.Infrastructure.Persistence.Repository.Equipment;
using Bookazone.Infrastructure.Persistence.Repository.Vehicles;
using Bookazone.Infrastructure.Persistence.Repository.Rentals;
using Bookazone.Infrastructure.Persistence.Repository.Reviews;
using Bookazone.Application.Services.Booking;
using Bookazone.Application.Services.Events;
using Bookazone.Application.Services.Equipment;
using Bookazone.Application.Services.Vehicles;
using Bookazone.Application.Services.Rentals;
using Bookazone.Application.Services.Reviews;
using Bookazone.Infrastructure.Services;
using Bookazone.Infrastructure.Services.Background;
using Bookazone.Infrastructure.Services.Files;
using Bookazone.Infrastructure.Services.JWT;
using Bookazone.Infrastructure.Services.Profile;
using Bookazone.Infrastructure.Services.Security;
using DinkToPdf;
using DinkToPdf.Contracts;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Functions = Bookazone.Application.Common.Shared.Utils.Functions;
using TokenRevocationService = Bookazone.Infrastructure.Services.TokenRevocationService;

namespace Bookazone.Infrastructure.Persistence.Dependency;

public static class DependencyInjection
{
    [Obsolete("Obsolete")]
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        
        var base64Key = configuration["Crypto:Base64Key"] ?? throw new InvalidOperationException("Missing encryption key in configuration.");
      //  Console.WriteLine(ConnectionStringProtector.EncryptConnectionString("Host=31.97.178.68;Database=Bookazone;Username=pcsolution;Password=KojoWest@2025?", base64Key));

        var encryptedConn = configuration["EncryptedConnectionStrings:Bookazone"] ?? throw new InvalidOperationException("Missing encrypted connection string.");
        var decryptedConn = ConnectionStringProtector.DecryptConnectionString(encryptedConn, base64Key);
        Console.WriteLine("<<<<<     =================================     >>>>>");
        Console.WriteLine(ConnectionStringProtector.DecryptConnectionString(encryptedConn, base64Key));
        services.AddSingleton<UtcDateTimeInterceptor>();
        services.AddDbContext<BookazoneDbContext>((sp, options) =>
        {
            options.UseNpgsql(decryptedConn);
            options.AddInterceptors(sp.GetRequiredService<UtcDateTimeInterceptor>());
        });
        

        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.AddSingleton(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var credentialPath = config["Firebase:CredentialPath"];
            if (string.IsNullOrWhiteSpace(credentialPath))
                throw new InvalidOperationException("Missing Firebase credential path in configuration.");

            var fullPath = Path.IsPathRooted(credentialPath)
                ? credentialPath
                : Path.Combine(Directory.GetCurrentDirectory(), credentialPath);

            if (!File.Exists(fullPath))
                throw new FileNotFoundException("Firebase credential file not found.", fullPath);

            GoogleCredential credential = GoogleCredential.FromFile(fullPath);
            return StorageClient.Create(credential);
        });
        services.AddHostedService<RefreshTokenCleanupJob>();
        services.AddScoped<FirebaseStorageService>();
        services.AddScoped<FirebaseNotificationService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<SubscriptionEmailService>();
        services.AddHostedService<SubscriptionBackgroundService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IAuthenticateService, AuthenticateService>();
        services.AddScoped<Functions>();
        services.AddScoped<IAuthorizationHandler, VerifiedDeviceHandler>(); 
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>(); 
        services.AddScoped<IJwtService, JwtService>(); 
        services.AddScoped<TokenRevocationService>();
        services.AddScoped<IUserContext, FunctionsUserContext>();
        services.AddHttpContextAccessor();
        services.AddControllers().AddJsonOptions(options => { options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); });
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "BookazoneAuthServer",
                    ValidAudience = "BookazoneClients",
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("Missing JWT encryption key in configuration."))),
                    RoleClaimType = "permission",
                    NameClaimType = ClaimTypes.Name
                };
            });
        
        
        
        services.AddAuthorization(options =>
        {
            foreach (var permission in typeof(Consts.Permissions).GetFields())
            {
                var permissionValue = permission.GetValue(null)?.ToString();
                if (!string.IsNullOrEmpty(permissionValue))
                {
                    options.AddPolicy(permissionValue, policy =>
                        policy.Requirements.Add(new PermissionRequirement(permissionValue)));
                }
            }
        });
        services.AddAuthorization(options =>
        {
            options.AddPolicy("VerifiedDeviceOnly", policy => policy.Requirements.Add(new VerifiedDeviceRequirement()));
        });
        
        services.Configure<PasswordHasherOptions>(options =>
        {
            options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
            options.IterationCount = 100_000;
        });

        
        
        services.Configure<JwtVerificationOptions>(configuration.GetSection("JwtVerification"));
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IUserVerificationService, UserVerificationService>();
        services.AddScoped<IUserPermissionService, UserPermissionService>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IUserAccessService, UserAccessService>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        services.AddScoped<IPasswordHasher<Users>, PasswordHasher<Users>>();
        
        
        services.AddScoped<ISocialAuthService, SocialAuthService>();
        services.AddScoped<IAuthProviderRepository, AuthProviderRepository>();
        services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();
        
        
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<IUserTenantRepository, UserTenantRepository>();
        services.AddScoped<ITenantWorkingHourRepository, TenantWorkingHourRepository>();
        services.AddScoped<ITenantSettingsRepository, TenantSettingsRepository>();
        services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
        services.AddScoped<ITenantSubscriptionRepository, TenantSubscriptionRepository>();
        services.AddScoped<IBookingCategoryRepository, BookingCategoryRepository>();
        services.AddScoped<ITenantBookingCategoryRepository, TenantBookingCategoryRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IBookingApprovalRepository, BookingApprovalRepository>();
        services.AddScoped<ITenantSportTypeRepository, TenantSportTypeRepository>();
        services.AddScoped<ISportResourceRepository, SportResourceRepository>();
        services.AddScoped<ISportResourceAvailabilityRepository, SportResourceAvailabilityRepository>();
        services.AddScoped<ISportResourceBlackoutRepository, SportResourceBlackoutRepository>();
        services.AddScoped<ISportResourcePricingRuleRepository, SportResourcePricingRuleRepository>();
        services.AddScoped<ITenantSportMediaRepository, TenantSportMediaRepository>();
        services.AddScoped<ISportResourceMediaRepository, SportResourceMediaRepository>();
        services.AddScoped<IEventCategoryRepository, EventCategoryRepository>();
        services.AddScoped<IEventCategorySubscriptionRepository, EventCategorySubscriptionRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IEventVenueRepository, EventVenueRepository>();
        services.AddScoped<IEventTicketTypeRepository, EventTicketTypeRepository>();
        services.AddScoped<IEventScheduleItemRepository, EventScheduleItemRepository>();
        services.AddScoped<IEventPolicyRepository, EventPolicyRepository>();
        services.AddScoped<IEventMediaRepository, EventMediaRepository>();
        services.AddScoped<IEventOrderRepository, EventOrderRepository>();
        services.AddScoped<IEventOrderItemRepository, EventOrderItemRepository>();
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<IVehiclePricingRuleRepository, VehiclePricingRuleRepository>();
        services.AddScoped<IVehicleSpecRepository, VehicleSpecRepository>();
        services.AddScoped<IVehiclePolicyRepository, VehiclePolicyRepository>();
        services.AddScoped<IVehicleMediaRepository, VehicleMediaRepository>();
        services.AddScoped<IVehicleOrderRepository, VehicleOrderRepository>();
        services.AddScoped<IEquipmentRepository, EquipmentRepository>();
        services.AddScoped<IEquipmentPricingRuleRepository, EquipmentPricingRuleRepository>();
        services.AddScoped<IEquipmentSpecRepository, EquipmentSpecRepository>();
        services.AddScoped<IEquipmentPolicyRepository, EquipmentPolicyRepository>();
        services.AddScoped<IEquipmentMediaRepository, EquipmentMediaRepository>();
        services.AddScoped<IEquipmentOrderRepository, EquipmentOrderRepository>();
        services.AddScoped<IRentalRepository, RentalRepository>();
        services.AddScoped<IRentalPricingRuleRepository, RentalPricingRuleRepository>();
        services.AddScoped<IRentalSpecRepository, RentalSpecRepository>();
        services.AddScoped<IRentalPolicyRepository, RentalPolicyRepository>();
        services.AddScoped<IRentalMediaRepository, RentalMediaRepository>();
        services.AddScoped<IRentalOrderRepository, RentalOrderRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IReviewAspectRatingRepository, ReviewAspectRatingRepository>();
        
        
        
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();        
        services.AddScoped<IStoredFileRepository, StoredFileRepository>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        
        services.AddScoped<IUserPermissionRepository, UserPermissionRepository>();
        services.AddScoped<IDeviceRepository, DeviceRepository>();
        services.AddScoped<IOneTimePasswordRepository, OneTimePasswordRepository>();
        services.AddScoped<IUserPasswordRepository, UserPasswordRepository>();
        services.AddScoped<IAppConfigRepository, AppConfigRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<ITenantDiscoveryService, TenantDiscoveryService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IBookingValidationService, BookingValidationService>();
        services.AddScoped<IBookingDiscoveryService, BookingDiscoveryService>();
        services.AddScoped<IBookingConfigurationService, BookingConfigurationService>();
        services.AddScoped<IEventDiscoveryService, EventDiscoveryService>();
        services.AddScoped<IEventManagementService, EventManagementService>();
        services.AddScoped<IEventCategoryService, EventCategoryService>();
        services.AddScoped<IEventOrderService, EventOrderService>();
        services.AddScoped<IEventSubscriptionService, EventSubscriptionService>();
        services.AddScoped<IEventNotificationService, EventNotificationService>();
        services.AddScoped<IEquipmentDiscoveryService, EquipmentDiscoveryService>();
        services.AddScoped<IEquipmentManagementService, EquipmentManagementService>();
        services.AddScoped<IEquipmentOrderService, EquipmentOrderService>();
        services.AddScoped<IVehicleDiscoveryService, VehicleDiscoveryService>();
        services.AddScoped<IVehicleManagementService, VehicleManagementService>();
        services.AddScoped<IVehicleOrderService, VehicleOrderService>();
        services.AddScoped<IRentalDiscoveryService, RentalDiscoveryService>();
        services.AddScoped<IRentalManagementService, RentalManagementService>();
        services.AddScoped<IRentalOrderService, RentalOrderService>();
        services.AddScoped<IReviewService, ReviewService>();
        
        services.AddSingleton<IConverter>(new SynchronizedConverter(new PdfTools()));
        services.AddSingleton<Base64Images>();
        
        
        //External API
        services.AddHttpClient<ICountryService, CountryService>();
        
        return services;
    }
}
