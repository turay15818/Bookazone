using Microsoft.EntityFrameworkCore;
using Bookazone.Api.Controllers.V1.Config.Request;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs.Converter;
using Bookazone.Application.DTOs.Response;
using Bookazone.Application.Interfaces.Repository.Booking;
using Bookazone.Application.Interfaces.Repository.Sports;
using Bookazone.Application.Interfaces.Repository.Subscription;
using Bookazone.Application.Interfaces.Repository.Tenant;
using Bookazone.Application.Interfaces.Services.Equipment;
using Bookazone.Application.Interfaces.Services.Events;
using Bookazone.Application.Interfaces.Services.Rentals;
using Bookazone.Application.Interfaces.Services.Tenant;
using Bookazone.Application.Interfaces.Services.Vehicles;
using Bookazone.Domain.Entities.Sports;
using Bookazone.Domain.Enums;

namespace Bookazone.Application.Services.Tenant;

public class TenantDiscoveryService(
    ITenantRepository tenantRepository,
    ITenantBookingCategoryRepository tenantBookingCategoryRepository,
    ITenantWorkingHourRepository tenantWorkingHourRepository,
    ITenantSettingsRepository tenantSettingsRepository,
    ILogger<TenantDiscoveryService> logger)
    : ITenantDiscoveryService
{
    public async Task<ApiResult> GetTenantFullDetailsAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var tenant = await tenantRepository.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == tenantId && t.Active && !t.Deleted, cancellationToken);

            if (tenant == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            var tenantDto = new VwTenantPublicDetail
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Description = tenant.Description,
                Email = tenant.Email,
                Phone = tenant.Phone,
                Address = tenant.Address,
                City = tenant.City,
                State = tenant.State,
                ZipCode = tenant.ZipCode,
                Country = tenant.Country,
                Logo = tenant.Logo,
                Subdomain = tenant.Subdomain,
                Code = tenant.Code,
                IsVerified = tenant.IsVerified,
                DateCreated = tenant.DateCreated
            };

            var tenantCategories = await tenantBookingCategoryRepository.Query()
                .AsNoTracking()
                .Include(tbc => tbc.FkBookingCategory)
                .Where(tbc =>
                    tbc.FkTenantId == tenantId &&
                    tbc.Active &&
                    !tbc.Deleted &&
                    tbc.FkBookingCategory.Active &&
                    !tbc.FkBookingCategory.Deleted)
                .ToListAsync(cancellationToken);

            var enabledCategories = tenantCategories
                .Where(tbc => tbc.IsEnabled)
                .ToList();

            var categoriesDto = enabledCategories
                .Select(BookingConfigConverter.ToDto)
                .ToList();

            var workingHours = await tenantWorkingHourRepository.GetByTenantAsync(tenantId, cancellationToken);
            var workingHoursDto = workingHours
                .Select(BookingConfigConverter.ToDto)
                .OrderBy(h => h.DayOfWeek)
                .ToList();

            var settings = await tenantSettingsRepository.GetByTenantAsync(tenantId);

            var response = new VwTenantFullPublic
            {
                Tenant = tenantDto,
                Categories = categoriesDto,
                WorkingHours = workingHoursDto,
                Settings = settings,
            };

            return ApiResponse.Success(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting public tenant details for {TenantId}", tenantId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

}