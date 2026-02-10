using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs;
using Bookazone.Application.DTOs.Converter;
using Bookazone.Application.DTOs.Response;
using Bookazone.Application.DTOs.Vehicles;
using Bookazone.Application.Interfaces.Context;
using Bookazone.Application.Interfaces.Repository.Booking;
using Bookazone.Application.Interfaces.Repository.Tenant;
using Bookazone.Application.Interfaces.Repository.Vehicles;
using Bookazone.Application.Interfaces.Services.Vehicles;
using Bookazone.Domain.Entities.Vehicles;
using Bookazone.Domain.Enums;

namespace Bookazone.Application.Services.Vehicles;

public class VehicleManagementService(
    IVehicleRepository vehicleRepository,
    IVehiclePricingRuleRepository vehiclePricingRuleRepository,
    IVehicleSpecRepository vehicleSpecRepository,
    IVehiclePolicyRepository vehiclePolicyRepository,
    IVehicleMediaRepository vehicleMediaRepository,
    ITenantRepository tenantRepository,
    ITenantBookingCategoryRepository tenantBookingCategoryRepository,
    IUserContext userContext,
    IHttpContextAccessor httpContextAccessor,
    ILogger<VehicleManagementService> logger)
    : IVehicleManagementService
{
    public async Task<ApiResult> GetTenantVehiclesAsync(
        List<VehicleStatus>? statuses = null,
        VehicleServiceType? serviceType = null,
        VehicleType? vehicleType = null,
        string? search = null,
        string? city = null,
        int? pageIndex = null,
        int? pageSize = null,
        string? sortBy = null,
        string? sortDirection = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!userContext.TenantId.HasValue)
                return ApiResponse.Error(ErrorHttp.Forbidden,
                    new Exception("User context does not contain a valid tenant id."));

            var tenantId = userContext.TenantId.Value;

            var query = vehicleRepository.Query()
                .AsNoTracking()
                .Include(v => v.FkTenant)
                .Include(v => v.FkCoverMedia)
                .ThenInclude(m => m.FkStoredFile)
                .Where(v =>
                    v.FkTenantId == tenantId &&
                    v.Active &&
                    !v.Deleted);

            if (statuses is { Count: > 0 })
                query = query.Where(v => statuses.Contains(v.Status));

            if (serviceType.HasValue)
                query = query.Where(v => v.ServiceType == serviceType.Value);

            if (vehicleType.HasValue)
                query = query.Where(v => v.Type == vehicleType.Value);

            if (!string.IsNullOrWhiteSpace(city))
            {
                var cityTerm = city.Trim().ToLowerInvariant();
                query = query.Where(v => v.City != null && v.City.ToLower().Contains(cityTerm));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLowerInvariant();
                query = query.Where(v =>
                    v.Title.ToLower().Contains(term) ||
                    (v.Subtitle != null && v.Subtitle.ToLower().Contains(term)) ||
                    (v.Make != null && v.Make.ToLower().Contains(term)) ||
                    (v.Model != null && v.Model.ToLower().Contains(term)) ||
                    v.FkTenant.Name.ToLower().Contains(term));
            }

            var sorted = ApplySorting(query, sortBy, sortDirection);
            var paged = await Paginate<Vehicle>.CreateAsync(sorted, pageIndex, pageSize);

            var pageData = paged.Data ?? new List<Vehicle>();
            var vehicleIds = pageData.Select(v => v.Id).ToList();
            var priceLookup = await BuildPriceLookupAsync(vehicleIds, cancellationToken);

            var dto = pageData.Select(v =>
            {
                var priceInfo = ResolvePrice(v, priceLookup);
                var card = VehicleConverter.ToListItemDto(
                    v,
                    v.FkTenant,
                    v.FkCoverMedia,
                    priceInfo.PriceFrom,
                    priceInfo.Currency,
                    priceInfo.Unit);
                card.CoverUrl = FileUrlHelper.BuildPublicUrl(
                    httpContextAccessor.HttpContext?.Request,
                    card.CoverUrl);
                return card;
            }).ToList();

            var response = new DataPaginate<VwVehicleListItem>
            {
                Data = dto,
                Pages = paged.Pages,
                PageIndex = paged.PageIndex
            };

            return ApiResponse.Success(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting tenant vehicles");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> GetVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var vehicle = await GetVehicleWithIncludesAsync(vehicleId, userContext.TenantId, cancellationToken);
            if (vehicle == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            var pricingRules = await vehiclePricingRuleRepository.GetByVehicleAsync(vehicleId, cancellationToken);
            var specs = await vehicleSpecRepository.GetByVehicleAsync(vehicleId, cancellationToken);
            var policies = await vehiclePolicyRepository.GetByVehicleAsync(vehicleId, cancellationToken);
            var media = await vehicleMediaRepository.GetByVehicleAsync(vehicleId, cancellationToken);

            var priceInfo = ResolvePrice(vehicle, pricingRules);
            var coverMedia = ResolveCover(vehicle, media);

            var mediaDto = media.Select(VehicleConverter.ToDto).ToList();
            foreach (var item in mediaDto)
                item.Url = FileUrlHelper.BuildPublicUrl(
                               httpContextAccessor.HttpContext?.Request,
                               item.Url) ?? string.Empty;

            var detail = VehicleConverter.ToDetailDto(
                vehicle,
                vehicle.FkTenant,
                coverMedia,
                pricingRules.Select(VehicleConverter.ToDto),
                specs.Select(VehicleConverter.ToDto),
                policies.Select(VehicleConverter.ToDto),
                mediaDto,
                priceInfo.PriceFrom,
                priceInfo.Currency,
                priceInfo.Unit);
            detail.CoverUrl = FileUrlHelper.BuildPublicUrl(
                httpContextAccessor.HttpContext?.Request,
                detail.CoverUrl);

            return ApiResponse.Success(detail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting vehicle {VehicleId}", vehicleId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
    public async Task<ApiResult> CreateVehicleAsync(VehicleCreateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var validation = ValidateVehicleRequest(request.Title);
            if (validation != null)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, validation);

            if (request.TenantId == Guid.Empty)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "TenantId is required.");

            if (IsTenantMismatch(request.TenantId))
                return ApiResponse.Error(ErrorHttp.Forbidden, new Exception("User is not authorized for this tenant."));

            if (!await tenantRepository.ExistsAsync(request.TenantId))
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (!await TenantSupportsVehiclesAsync(request.TenantId, cancellationToken))
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Tenant is not enabled for vehicles.");

            var vehicle = new Vehicle
            {
                FkTenantId = request.TenantId,
                ServiceType = request.ServiceType,
                Type = request.Type,
                Title = request.Title.Trim(),
                Subtitle = request.Subtitle?.Trim(),
                Description = request.Description?.Trim(),
                City = request.City?.Trim(),
                Address = request.Address?.Trim(),
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Make = request.Make?.Trim(),
                Model = request.Model?.Trim(),
                Year = request.Year,
                Seats = request.Seats,
                Transmission = request.Transmission,
                FuelType = request.FuelType,
                HasAirConditioning = request.HasAirConditioning,
                HasInsurance = request.HasInsurance,
                DeliveryAvailable = request.DeliveryAvailable,
                DriverOption = request.DriverOption,
                FuelPolicy = request.FuelPolicy?.Trim(),
                MinBookingDuration = request.MinBookingDuration,
                MaxBookingDuration = request.MaxBookingDuration,
                BookingDurationUnit = request.BookingDurationUnit,
                MaxLoadTons = request.MaxLoadTons,
                CargoVolumeCubicMeters = request.CargoVolumeCubicMeters,
                BookingMode = request.BookingMode,
                Status = VehicleStatus.Draft,
                PriceFrom = request.PriceFrom,
                Currency = NormalizeCurrency(request.Currency),
                Active = true,
                Deleted = false
            };

            await using var transaction = await vehicleRepository.BeginTransactionAsync();
            var created = await vehicleRepository.AddAsync(vehicle);

            if (request.PricingRules is { Count: > 0 })
                await CreatePricingRulesAsync(created.Id, request.PricingRules, cancellationToken);

            if (request.Specs is { Count: > 0 })
                await CreateSpecsAsync(created.Id, request.Specs, cancellationToken);

            if (request.Policies is { Count: > 0 })
                await CreatePoliciesAsync(created.Id, request.Policies, cancellationToken);

            if (request.Media is { Count: > 0 })
                await CreateMediaAsync(created.Id, request.Media, cancellationToken);

            await UpdateVehiclePriceFromAsync(created, force: request.PriceFrom == null, cancellationToken);
            await UpdateVehicleCoverAsync(created, cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return await GetVehicleAsync(created.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating vehicle for tenant {TenantId}", request.TenantId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> UpdateVehicleAsync(VehicleUpdateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var validation = ValidateVehicleRequest(request.Title);
            if (validation != null)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, validation);

            if (request.TenantId == Guid.Empty)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "TenantId is required.");

            if (IsTenantMismatch(request.TenantId))
                return ApiResponse.Error(ErrorHttp.Forbidden, new Exception("User is not authorized for this tenant."));

            var vehicle = await GetVehicleForUpdateAsync(request.Id, request.TenantId, cancellationToken);
            if (vehicle == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            vehicle.ServiceType = request.ServiceType;
            vehicle.Type = request.Type;
            vehicle.Title = request.Title.Trim();
            vehicle.Subtitle = request.Subtitle?.Trim();
            vehicle.Description = request.Description?.Trim();
            vehicle.City = request.City?.Trim();
            vehicle.Address = request.Address?.Trim();
            vehicle.Latitude = request.Latitude;
            vehicle.Longitude = request.Longitude;
            vehicle.Make = request.Make?.Trim();
            vehicle.Model = request.Model?.Trim();
            vehicle.Year = request.Year;
            vehicle.Seats = request.Seats;
            vehicle.Transmission = request.Transmission;
            vehicle.FuelType = request.FuelType;
            vehicle.HasAirConditioning = request.HasAirConditioning;
            vehicle.HasInsurance = request.HasInsurance;
            vehicle.DeliveryAvailable = request.DeliveryAvailable;
            vehicle.DriverOption = request.DriverOption;
            vehicle.FuelPolicy = request.FuelPolicy?.Trim();
            vehicle.MinBookingDuration = request.MinBookingDuration;
            vehicle.MaxBookingDuration = request.MaxBookingDuration;
            vehicle.BookingDurationUnit = request.BookingDurationUnit;
            vehicle.MaxLoadTons = request.MaxLoadTons;
            vehicle.CargoVolumeCubicMeters = request.CargoVolumeCubicMeters;
            vehicle.BookingMode = request.BookingMode;
            vehicle.PriceFrom = request.PriceFrom;
            vehicle.Currency = NormalizeCurrency(request.Currency);

            await vehicleRepository.UpdateAsync(vehicle);
            await UpdateVehiclePriceFromAsync(vehicle, force: request.PriceFrom == null, cancellationToken);
            await UpdateVehicleCoverAsync(vehicle, cancellationToken);

            return await GetVehicleAsync(vehicle.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating vehicle {VehicleId}", request.Id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> PublishVehicleAsync(VehiclePublishRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var vehicle = await GetVehicleForUpdateAsync(request.VehicleId, userContext.TenantId, cancellationToken);
            if (vehicle == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (!await TenantSupportsVehiclesAsync(vehicle.FkTenantId, cancellationToken))
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Tenant is not enabled for vehicles.");

            if (vehicle.Status == VehicleStatus.Published)
                return ApiResponse.Success(true);

            vehicle.Status = VehicleStatus.Published;
            await vehicleRepository.UpdateAsync(vehicle);

            return ApiResponse.Success(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing vehicle {VehicleId}", request.VehicleId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> UpdateVehicleStatusAsync(VehicleStatusUpdateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var vehicle = await GetVehicleForUpdateAsync(request.VehicleId, userContext.TenantId, cancellationToken);
            if (vehicle == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            vehicle.Status = request.Status;
            await vehicleRepository.UpdateAsync(vehicle);

            return ApiResponse.Success(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating vehicle status {VehicleId}", request.VehicleId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> DeleteVehicleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var vehicle = await GetVehicleForUpdateAsync(id, userContext.TenantId, cancellationToken);
            if (vehicle == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            var deleted = await vehicleRepository.DeleteAsync(id);
            return deleted
                ? ApiResponse.Success(true)
                : ApiResponse.Error(ErrorHttp.DbDeleteError);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting vehicle {VehicleId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
    public async Task<ApiResult> UpsertVehiclePricingRulesAsync(
        Guid vehicleId,
        List<VehiclePricingRuleUpsertRequest> requests,
        bool allOrNothing = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (requests == null || requests.Count == 0)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Request list is empty.");

            var vehicle = await GetVehicleForUpdateAsync(vehicleId, userContext.TenantId, cancellationToken);
            if (vehicle == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            await using IDbContextTransaction? transaction = allOrNothing
                ? await vehiclePricingRuleRepository.BeginTransactionAsync()
                : null;

            var results = new List<object>();
            foreach (var request in requests)
            {
                var result = await UpsertVehiclePricingRuleAsync(vehicleId, request, cancellationToken);
                if (result.Status !=1)
                {
                    if (transaction != null) await transaction.RollbackAsync(cancellationToken);
                    return result;
                }

                results.Add(result);
            }

            if (transaction != null) await transaction.CommitAsync(cancellationToken);

            await UpdateVehiclePriceFromAsync(vehicle, force: true, cancellationToken);

            return ApiResponse.Success(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error upserting pricing rules for vehicle {VehicleId}", vehicleId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> UpsertVehicleSpecsAsync(
        Guid vehicleId,
        List<VehicleSpecUpsertRequest> requests,
        bool allOrNothing = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (requests == null || requests.Count == 0)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Request list is empty.");

            var vehicle = await GetVehicleForUpdateAsync(vehicleId, userContext.TenantId, cancellationToken);
            if (vehicle == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            await using IDbContextTransaction? transaction = allOrNothing
                ? await vehicleSpecRepository.BeginTransactionAsync()
                : null;

            var results = new List<object>();
            foreach (var request in requests)
            {
                var result = await UpsertVehicleSpecAsync(vehicleId, request, cancellationToken);
                if (result.Status !=1)
                {
                    if (transaction != null) await transaction.RollbackAsync(cancellationToken);
                    return result;
                }

                 results.Add(result);
            }

            if (transaction != null) await transaction.CommitAsync(cancellationToken);

            return ApiResponse.Success(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error upserting specs for vehicle {VehicleId}", vehicleId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> UpsertVehiclePoliciesAsync(
        Guid vehicleId,
        List<VehiclePolicyUpsertRequest> requests,
        bool allOrNothing = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (requests == null || requests.Count == 0)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Request list is empty.");

            var vehicle = await GetVehicleForUpdateAsync(vehicleId, userContext.TenantId, cancellationToken);
            if (vehicle == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            await using IDbContextTransaction? transaction = allOrNothing
                ? await vehiclePolicyRepository.BeginTransactionAsync()
                : null;

            var results = new List<object>();
            foreach (var request in requests)
            {
                var result = await UpsertVehiclePolicyAsync(vehicleId, request, cancellationToken);
                if (result.Status !=1)
                {
                    if (transaction != null) await transaction.RollbackAsync(cancellationToken);
                    return result;
                }

                results.Add(result);
            }

            if (transaction != null) await transaction.CommitAsync(cancellationToken);

            return ApiResponse.Success(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error upserting policies for vehicle {VehicleId}", vehicleId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> UpsertVehicleMediaAsync(
        Guid vehicleId,
        List<VehicleMediaUpsertRequest> requests,
        bool allOrNothing = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (requests == null || requests.Count == 0)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Request list is empty.");

            var vehicle = await GetVehicleForUpdateAsync(vehicleId, userContext.TenantId, cancellationToken);
            if (vehicle == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            await using IDbContextTransaction? transaction = allOrNothing
                ? await vehicleMediaRepository.BeginTransactionAsync()
                : null;

            var results = new List<object>();
            foreach (var request in requests)
            {
                var result = await UpsertVehicleMediaItemAsync(vehicleId, request, cancellationToken);
                if (result.Status !=1)
                {
                    if (transaction != null) await transaction.RollbackAsync(cancellationToken);
                    return result;
                }

                results.Add(result);
            }

            if (transaction != null) await transaction.CommitAsync(cancellationToken);

            await UpdateVehicleCoverAsync(vehicle, cancellationToken);

            return ApiResponse.Success(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error upserting media for vehicle {VehicleId}", vehicleId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
    private async Task<ApiResult> UpsertVehiclePricingRuleAsync(
        Guid vehicleId,
        VehiclePricingRuleUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var validation = ValidatePricingRuleRequest(request);
        if (validation != null)
            return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, validation);

        if (request.Id.HasValue && request.Id.Value != Guid.Empty)
        {
            var existing = await vehiclePricingRuleRepository.GetByIdAsync(request.Id.Value);
            if (existing == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (existing.FkVehicleId != vehicleId)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Pricing rule does not belong to this vehicle.");

            existing.Unit = request.Unit;
            existing.PriceAmount = request.PriceAmount;
            existing.Currency = NormalizeCurrency(request.Currency);
            existing.MinQuantity = request.MinQuantity;
            existing.MaxQuantity = request.MaxQuantity;
            existing.IsPrimary = request.IsPrimary;

            var updated = await vehiclePricingRuleRepository.UpdateAsync(existing);
            return ApiResponse.Success(VehicleConverter.ToDto(updated));
        }

        var created = new VehiclePricingRule
        {
            FkVehicleId = vehicleId,
            Unit = request.Unit,
            PriceAmount = request.PriceAmount,
            Currency = NormalizeCurrency(request.Currency),
            MinQuantity = request.MinQuantity,
            MaxQuantity = request.MaxQuantity,
            IsPrimary = request.IsPrimary,
            Active = true,
            Deleted = false
        };

        var added = await vehiclePricingRuleRepository.AddAsync(created);
        return ApiResponse.Success(VehicleConverter.ToDto(added));
    }

    private async Task<ApiResult> UpsertVehicleSpecAsync(
        Guid vehicleId,
        VehicleSpecUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var validation = ValidateSpecRequest(request);
        if (validation != null)
            return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, validation);

        if (request.Id.HasValue && request.Id.Value != Guid.Empty)
        {
            var existing = await vehicleSpecRepository.GetByIdAsync(request.Id.Value);
            if (existing == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (existing.FkVehicleId != vehicleId)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Spec does not belong to this vehicle.");

            existing.Label = request.Label.Trim();
            existing.Value = request.Value.Trim();
            existing.SortOrder = request.SortOrder;

            var updated = await vehicleSpecRepository.UpdateAsync(existing);
            return ApiResponse.Success(VehicleConverter.ToDto(updated));
        }

        var created = new VehicleSpec
        {
            FkVehicleId = vehicleId,
            Label = request.Label.Trim(),
            Value = request.Value.Trim(),
            SortOrder = request.SortOrder,
            Active = true,
            Deleted = false
        };

        var added = await vehicleSpecRepository.AddAsync(created);
        return ApiResponse.Success(VehicleConverter.ToDto(added));
    }

    private async Task<ApiResult> UpsertVehiclePolicyAsync(
        Guid vehicleId,
        VehiclePolicyUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var validation = ValidatePolicyRequest(request);
        if (validation != null)
            return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, validation);

        if (request.Id.HasValue && request.Id.Value != Guid.Empty)
        {
            var existing = await vehiclePolicyRepository.GetByIdAsync(request.Id.Value);
            if (existing == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (existing.FkVehicleId != vehicleId)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Policy does not belong to this vehicle.");

            existing.Title = request.Title.Trim();
            existing.Body = request.Body?.Trim();
            existing.SortOrder = request.SortOrder;

            var updated = await vehiclePolicyRepository.UpdateAsync(existing);
            return ApiResponse.Success(VehicleConverter.ToDto(updated));
        }

        var created = new VehiclePolicy
        {
            FkVehicleId = vehicleId,
            Title = request.Title.Trim(),
            Body = request.Body?.Trim(),
            SortOrder = request.SortOrder,
            Active = true,
            Deleted = false
        };

        var added = await vehiclePolicyRepository.AddAsync(created);
        return ApiResponse.Success(VehicleConverter.ToDto(added));
    }

    private async Task<ApiResult> UpsertVehicleMediaItemAsync(
        Guid vehicleId,
        VehicleMediaUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var validation = ValidateMediaRequest(request);
        if (validation != null)
            return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, validation);

        if (request.Id.HasValue && request.Id.Value != Guid.Empty)
        {
            var existing = await vehicleMediaRepository.GetByIdAsync(request.Id.Value);
            if (existing == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (existing.FkVehicleId != vehicleId)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Media does not belong to this vehicle.");

            existing.FkStoredFileId = request.StoredFileId;
            existing.IsCover = request.IsCover;
            existing.SortOrder = request.SortOrder;

            var updated = await vehicleMediaRepository.UpdateAsync(existing);
            return ApiResponse.Success(VehicleConverter.ToDto(updated));
        }

        var created = new VehicleMedia
        {
            FkVehicleId = vehicleId,
            FkStoredFileId = request.StoredFileId,
            IsCover = request.IsCover,
            SortOrder = request.SortOrder,
            Active = true,
            Deleted = false
        };

        var added = await vehicleMediaRepository.AddAsync(created);
        return ApiResponse.Success(VehicleConverter.ToDto(added));
    }
    private async Task CreatePricingRulesAsync(
        Guid vehicleId,
        List<VehiclePricingRuleUpsertRequest> requests,
        CancellationToken cancellationToken)
    {
        foreach (var request in requests)
        {
            var validation = ValidatePricingRuleRequest(request);
            if (validation != null)
                throw new InvalidOperationException(validation);

            var created = new VehiclePricingRule
            {
                FkVehicleId = vehicleId,
                Unit = request.Unit,
                PriceAmount = request.PriceAmount,
                Currency = NormalizeCurrency(request.Currency),
                MinQuantity = request.MinQuantity,
                MaxQuantity = request.MaxQuantity,
                IsPrimary = request.IsPrimary,
                Active = true,
                Deleted = false
            };

            await vehiclePricingRuleRepository.AddAsync(created);
        }
    }

    private async Task CreateSpecsAsync(
        Guid vehicleId,
        List<VehicleSpecUpsertRequest> requests,
        CancellationToken cancellationToken)
    {
        foreach (var request in requests)
        {
            var validation = ValidateSpecRequest(request);
            if (validation != null)
                throw new InvalidOperationException(validation);

            var created = new VehicleSpec
            {
                FkVehicleId = vehicleId,
                Label = request.Label.Trim(),
                Value = request.Value.Trim(),
                SortOrder = request.SortOrder,
                Active = true,
                Deleted = false
            };

            await vehicleSpecRepository.AddAsync(created);
        }
    }

    private async Task CreatePoliciesAsync(
        Guid vehicleId,
        List<VehiclePolicyUpsertRequest> requests,
        CancellationToken cancellationToken)
    {
        foreach (var request in requests)
        {
            var validation = ValidatePolicyRequest(request);
            if (validation != null)
                throw new InvalidOperationException(validation);

            var created = new VehiclePolicy
            {
                FkVehicleId = vehicleId,
                Title = request.Title.Trim(),
                Body = request.Body?.Trim(),
                SortOrder = request.SortOrder,
                Active = true,
                Deleted = false
            };

            await vehiclePolicyRepository.AddAsync(created);
        }
    }

    private async Task CreateMediaAsync(
        Guid vehicleId,
        List<VehicleMediaUpsertRequest> requests,
        CancellationToken cancellationToken)
    {
        foreach (var request in requests)
        {
            var validation = ValidateMediaRequest(request);
            if (validation != null)
                throw new InvalidOperationException(validation);

            var created = new VehicleMedia
            {
                FkVehicleId = vehicleId,
                FkStoredFileId = request.StoredFileId,
                IsCover = request.IsCover,
                SortOrder = request.SortOrder,
                Active = true,
                Deleted = false
            };

            await vehicleMediaRepository.AddAsync(created);
        }
    }
    private async Task<Vehicle?> GetVehicleWithIncludesAsync(
        Guid vehicleId,
        Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var query = vehicleRepository.Query()
            .AsNoTracking()
            .Include(v => v.FkTenant)
            .Include(v => v.FkCoverMedia)
            .ThenInclude(m => m.FkStoredFile)
            .Where(v =>
                v.Id == vehicleId &&
                v.Active &&
                !v.Deleted);

        if (tenantId.HasValue)
            query = query.Where(v => v.FkTenantId == tenantId.Value);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<Vehicle?> GetVehicleForUpdateAsync(
        Guid vehicleId,
        Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var query = vehicleRepository.Query()
            .Where(v =>
                v.Id == vehicleId &&
                v.Active &&
                !v.Deleted);

        if (tenantId.HasValue)
            query = query.Where(v => v.FkTenantId == tenantId.Value);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<bool> TenantSupportsVehiclesAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var code = BookingCategoryTypeMapper.ToCode(BookingCategoryType.Vehicles);
        return await tenantBookingCategoryRepository.Query()
            .AsNoTracking()
            .AnyAsync(tbc =>
                    tbc.FkTenantId == tenantId &&
                    tbc.IsEnabled &&
                    tbc.Active &&
                    !tbc.Deleted &&
                    tbc.FkBookingCategory.Code == code &&
                    tbc.FkBookingCategory.Active &&
                    !tbc.FkBookingCategory.Deleted,
                cancellationToken);
    }

    private async Task UpdateVehiclePriceFromAsync(Vehicle vehicle, bool force, CancellationToken cancellationToken)
    {
        if (!force && vehicle.PriceFrom.HasValue)
            return;

        var pricingRules = await vehiclePricingRuleRepository.Query()
            .AsNoTracking()
            .Where(r =>
                r.FkVehicleId == vehicle.Id &&
                r.Active &&
                !r.Deleted)
            .ToListAsync(cancellationToken);

        if (pricingRules.Count == 0)
            return;

        var primary = pricingRules.FirstOrDefault(r => r.IsPrimary) ??
                      pricingRules.OrderBy(r => r.PriceAmount).First();

        vehicle.PriceFrom = primary.PriceAmount;
        vehicle.Currency = primary.Currency;
        await vehicleRepository.UpdateAsync(vehicle);
    }

    private async Task UpdateVehicleCoverAsync(Vehicle vehicle, CancellationToken cancellationToken)
    {
        var cover = await vehicleMediaRepository.Query()
            .AsNoTracking()
            .Where(m =>
                m.FkVehicleId == vehicle.Id &&
                m.Active &&
                !m.Deleted &&
                m.IsCover)
            .OrderBy(m => m.SortOrder)
            .FirstOrDefaultAsync(cancellationToken);

        if (cover == null)
        {
            cover = await vehicleMediaRepository.Query()
                .AsNoTracking()
                .Where(m =>
                    m.FkVehicleId == vehicle.Id &&
                    m.Active &&
                    !m.Deleted)
                .OrderBy(m => m.SortOrder)
                .FirstOrDefaultAsync(cancellationToken);
        }

        vehicle.FkCoverMediaId = cover?.Id;
        await vehicleRepository.UpdateAsync(vehicle);
    }

    private static VehicleMedia? ResolveCover(Vehicle vehicle, List<VehicleMedia> media)
    {
        if (vehicle.FkCoverMediaId.HasValue)
            return media.FirstOrDefault(m => m.Id == vehicle.FkCoverMediaId.Value);

        return media.FirstOrDefault(m => m.IsCover) ?? media.FirstOrDefault();
    }
    private static IQueryable<Vehicle> ApplySorting(IQueryable<Vehicle> query, string? sortBy, string? sortDirection)
    {
        var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        return (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "title" => isDesc ? query.OrderByDescending(v => v.Title) : query.OrderBy(v => v.Title),
            "price" => isDesc ? query.OrderByDescending(v => v.PriceFrom) : query.OrderBy(v => v.PriceFrom),
            "status" => isDesc ? query.OrderByDescending(v => v.Status) : query.OrderBy(v => v.Status),
            "city" => isDesc ? query.OrderByDescending(v => v.City) : query.OrderBy(v => v.City),
            _ => isDesc ? query.OrderByDescending(v => v.DateCreated) : query.OrderBy(v => v.DateCreated)
        };
    }

    private async Task<Dictionary<Guid, PriceInfo>> BuildPriceLookupAsync(
        List<Guid> vehicleIds,
        CancellationToken cancellationToken)
    {
        if (vehicleIds.Count == 0)
            return new Dictionary<Guid, PriceInfo>();

        var rules = await vehiclePricingRuleRepository.Query()
            .AsNoTracking()
            .Where(r =>
                vehicleIds.Contains(r.FkVehicleId) &&
                r.Active &&
                !r.Deleted)
            .Select(r => new { r.FkVehicleId, r.PriceAmount, r.Currency, r.Unit, r.IsPrimary })
            .ToListAsync(cancellationToken);

        return rules
            .GroupBy(r => r.FkVehicleId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var primary = g.FirstOrDefault(x => x.IsPrimary) ??
                                  g.OrderBy(x => x.PriceAmount).First();
                    return new PriceInfo(primary.PriceAmount, primary.Currency, primary.Unit);
                });
    }

    private static PriceInfo ResolvePrice(Vehicle vehicle, Dictionary<Guid, PriceInfo> lookup)
    {
        if (lookup.TryGetValue(vehicle.Id, out var info))
            return info;

        return new PriceInfo(vehicle.PriceFrom, vehicle.Currency, null);
    }

    private static PriceInfo ResolvePrice(Vehicle vehicle, IReadOnlyCollection<VehiclePricingRule> rules)
    {
        if (rules.Count == 0)
            return new PriceInfo(vehicle.PriceFrom, vehicle.Currency, null);

        var primary = rules.FirstOrDefault(r => r.IsPrimary) ??
                      rules.OrderBy(r => r.PriceAmount).First();

        return new PriceInfo(primary.PriceAmount, primary.Currency, primary.Unit);
    }

    private static string? ValidateVehicleRequest(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return "Title is required.";

        return null;
    }

    private static string? ValidatePricingRuleRequest(VehiclePricingRuleUpsertRequest request)
    {
        if (request.PriceAmount < 0)
            return "PriceAmount must be zero or greater.";

        if (request.MinQuantity.HasValue && request.MinQuantity.Value < 0)
            return "MinQuantity cannot be negative.";

        if (request.MaxQuantity.HasValue && request.MaxQuantity.Value < 0)
            return "MaxQuantity cannot be negative.";

        if (request.MinQuantity.HasValue && request.MaxQuantity.HasValue &&
            request.MinQuantity.Value > request.MaxQuantity.Value)
            return "MinQuantity must be less than or equal to MaxQuantity.";

        return null;
    }

    private static string? ValidateSpecRequest(VehicleSpecUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Label))
            return "Spec label is required.";

        if (string.IsNullOrWhiteSpace(request.Value))
            return "Spec value is required.";

        return null;
    }

    private static string? ValidatePolicyRequest(VehiclePolicyUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return "Policy title is required.";

        return null;
    }

    private static string? ValidateMediaRequest(VehicleMediaUpsertRequest request)
    {
        if (request.StoredFileId == Guid.Empty)
            return "StoredFileId is required.";

        return null;
    }

    private bool IsTenantMismatch(Guid tenantId)
    {
        return userContext.TenantId.HasValue && userContext.TenantId.Value != tenantId;
    }

    private static string NormalizeCurrency(string? currency)
    {
        return string.IsNullOrWhiteSpace(currency)
            ? "USD"
            : currency.Trim().ToUpperInvariant();
    }

    private sealed record PriceInfo(decimal? PriceFrom, string? Currency, VehiclePricingUnit? Unit);
}
