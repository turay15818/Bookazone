using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs;
using Bookazone.Application.DTOs.Converter;
using Bookazone.Application.DTOs.Response;
using Bookazone.Application.DTOs.Rentals;
using Bookazone.Application.Interfaces.Context;
using Bookazone.Application.Interfaces.Repository.Booking;
using Bookazone.Application.Interfaces.Repository.Tenant;
using Bookazone.Application.Interfaces.Repository.Rentals;
using Bookazone.Application.Interfaces.Services.Rentals;
using Bookazone.Domain.Entities.Rentals;
using Bookazone.Domain.Enums;

namespace Bookazone.Application.Services.Rentals;

public class RentalManagementService(
    IRentalRepository rentalRepository,
    IRentalPricingRuleRepository rentalPricingRuleRepository,
    IRentalSpecRepository rentalSpecRepository,
    IRentalPolicyRepository rentalPolicyRepository,
    IRentalMediaRepository rentalMediaRepository,
    ITenantRepository tenantRepository,
    ITenantBookingCategoryRepository tenantBookingCategoryRepository,
    IUserContext userContext,
    IHttpContextAccessor httpContextAccessor,
    ILogger<RentalManagementService> logger)
    : IRentalManagementService
{
    public async Task<ApiResult> GetTenantRentalsAsync(
        List<RentalStatus>? statuses = null,
        RentalType? type = null,
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

            var query = rentalRepository.Query()
                .AsNoTracking()
                .Include(r => r.FkTenant)
                .Include(r => r.FkCoverMedia)
                .ThenInclude(m => m.FkStoredFile)
                .Where(r =>
                    r.FkTenantId == tenantId &&
                    r.Active &&
                    !r.Deleted);

            if (statuses is { Count: > 0 })
                query = query.Where(r => statuses.Contains(r.Status));

            if (type.HasValue)
                query = query.Where(r => r.Type == type.Value);

            if (!string.IsNullOrWhiteSpace(city))
            {
                var cityTerm = city.Trim().ToLowerInvariant();
                query = query.Where(r => r.City != null && r.City.ToLower().Contains(cityTerm));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLowerInvariant();
                query = query.Where(r =>
                    r.Title.ToLower().Contains(term) ||
                    (r.Subtitle != null && r.Subtitle.ToLower().Contains(term)) ||
                    (r.Address != null && r.Address.ToLower().Contains(term)) ||
                    (r.City != null && r.City.ToLower().Contains(term)) ||
                    r.FkTenant.Name.ToLower().Contains(term));
            }

            var sorted = ApplySorting(query, sortBy, sortDirection);
            var paged = await Paginate<Rental>.CreateAsync(sorted, pageIndex, pageSize);

            var pageData = paged.Data ?? new List<Rental>();
            var rentalIds = pageData.Select(r => r.Id).ToList();
            var priceLookup = await BuildPriceLookupAsync(rentalIds, cancellationToken);

            var dto = pageData.Select(r =>
            {
                var priceInfo = ResolvePrice(r, priceLookup);
                var card = RentalConverter.ToListItemDto(
                    r,
                    r.FkTenant,
                    r.FkCoverMedia,
                    priceInfo.PriceFrom,
                    priceInfo.Currency,
                    priceInfo.Unit);
                card.CoverUrl = FileUrlHelper.BuildPublicUrl(
                    httpContextAccessor.HttpContext?.Request,
                    card.CoverUrl);
                return card;
            }).ToList();

            var response = new DataPaginate<VwRentalListItem>
            {
                Data = dto,
                Pages = paged.Pages,
                PageIndex = paged.PageIndex
            };

            return ApiResponse.Success(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting tenant rentals");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> GetRentalAsync(Guid rentalId, CancellationToken cancellationToken = default)
    {
        try
        {
            var rental = await GetRentalWithIncludesAsync(rentalId, userContext.TenantId, cancellationToken);
            if (rental == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            var pricingRules = await rentalPricingRuleRepository.GetByRentalAsync(rentalId, cancellationToken);
            var specs = await rentalSpecRepository.GetByRentalAsync(rentalId, cancellationToken);
            var policies = await rentalPolicyRepository.GetByRentalAsync(rentalId, cancellationToken);
            var media = await rentalMediaRepository.GetByRentalAsync(rentalId, cancellationToken);

            var priceInfo = ResolvePrice(rental, pricingRules);
            var coverMedia = ResolveCover(rental, media);

            var mediaDto = media.Select(RentalConverter.ToDto).ToList();
            foreach (var item in mediaDto)
                item.Url = FileUrlHelper.BuildPublicUrl(
                               httpContextAccessor.HttpContext?.Request,
                               item.Url) ?? string.Empty;

            var detail = RentalConverter.ToDetailDto(
                rental,
                rental.FkTenant,
                coverMedia,
                pricingRules.Select(RentalConverter.ToDto),
                specs.Select(RentalConverter.ToDto),
                policies.Select(RentalConverter.ToDto),
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
            logger.LogError(ex, "Error getting rental {rentalId}", rentalId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
    public async Task<ApiResult> CreateRentalAsync(RentalCreateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var validation = ValidateRentalRequest(request.Title, request.UnitsAvailable);
            if (validation != null)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, validation);

            if (request.TenantId == Guid.Empty)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "TenantId is required.");

            if (IsTenantMismatch(request.TenantId))
                return ApiResponse.Error(ErrorHttp.Forbidden, new Exception("User is not authorized for this tenant."));

            if (!await tenantRepository.ExistsAsync(request.TenantId))
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (!await TenantSupportsRentalsAsync(request.TenantId, cancellationToken))
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Tenant is not enabled for rentals.");

            var rental = new Rental
            {
                FkTenantId = request.TenantId,
                Type = request.Type,
                Title = request.Title.Trim(),
                Subtitle = request.Subtitle?.Trim(),
                Description = request.Description?.Trim(),
                City = request.City?.Trim(),
                Address = request.Address?.Trim(),
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Capacity = request.Capacity,
                Bedrooms = request.Bedrooms,
                Bathrooms = request.Bathrooms,
                FloorAreaSquareMeters = request.FloorAreaSquareMeters,
                UnitsAvailable = request.UnitsAvailable,
                HasToilet = request.HasToilet,
                HasAirConditioning = request.HasAirConditioning,
                HasFan = request.HasFan,
                HasSoundSystem = request.HasSoundSystem,
                HasWifi = request.HasWifi,
                HasParking = request.HasParking,
                MinBookingDuration = request.MinBookingDuration,
                MaxBookingDuration = request.MaxBookingDuration,
                BookingDurationUnit = request.BookingDurationUnit,
                CheckInTime = request.CheckInTime,
                CheckOutTime = request.CheckOutTime,
                BookingMode = request.BookingMode,
                Status = RentalStatus.Draft,
                PriceFrom = request.PriceFrom,
                Currency = NormalizeCurrency(request.Currency),
                Active = true,
                Deleted = false
            };

            await using var transaction = await rentalRepository.BeginTransactionAsync();
            var created = await rentalRepository.AddAsync(rental);

            if (request.PricingRules is { Count: > 0 })
                await CreatePricingRulesAsync(created.Id, request.PricingRules, cancellationToken);

            if (request.Specs is { Count: > 0 })
                await CreateSpecsAsync(created.Id, request.Specs, cancellationToken);

            if (request.Policies is { Count: > 0 })
                await CreatePoliciesAsync(created.Id, request.Policies, cancellationToken);

            if (request.Media is { Count: > 0 })
                await CreateMediaAsync(created.Id, request.Media, cancellationToken);

            await UpdateRentalPriceFromAsync(created, force: request.PriceFrom == null, cancellationToken);
            await UpdateRentalCoverAsync(created, cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return await GetRentalAsync(created.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating rental for tenant {TenantId}", request.TenantId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> UpdateRentalAsync(RentalUpdateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var validation = ValidateRentalRequest(request.Title, request.UnitsAvailable);
            if (validation != null)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, validation);

            if (request.TenantId == Guid.Empty)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "TenantId is required.");

            if (IsTenantMismatch(request.TenantId))
                return ApiResponse.Error(ErrorHttp.Forbidden, new Exception("User is not authorized for this tenant."));

            var rental = await GetRentalForUpdateAsync(request.Id, request.TenantId, cancellationToken);
            if (rental == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            rental.Type = request.Type;
            rental.Title = request.Title.Trim();
            rental.Subtitle = request.Subtitle?.Trim();
            rental.Description = request.Description?.Trim();
            rental.City = request.City?.Trim();
            rental.Address = request.Address?.Trim();
            rental.Latitude = request.Latitude;
            rental.Longitude = request.Longitude;
            rental.Capacity = request.Capacity;
            rental.Bedrooms = request.Bedrooms;
            rental.Bathrooms = request.Bathrooms;
            rental.FloorAreaSquareMeters = request.FloorAreaSquareMeters;
            rental.UnitsAvailable = request.UnitsAvailable;
            rental.HasToilet = request.HasToilet;
            rental.HasAirConditioning = request.HasAirConditioning;
            rental.HasFan = request.HasFan;
            rental.HasSoundSystem = request.HasSoundSystem;
            rental.HasWifi = request.HasWifi;
            rental.HasParking = request.HasParking;
            rental.MinBookingDuration = request.MinBookingDuration;
            rental.MaxBookingDuration = request.MaxBookingDuration;
            rental.BookingDurationUnit = request.BookingDurationUnit;
            rental.CheckInTime = request.CheckInTime;
            rental.CheckOutTime = request.CheckOutTime;
            rental.BookingMode = request.BookingMode;
            rental.PriceFrom = request.PriceFrom;
            rental.Currency = NormalizeCurrency(request.Currency);

            await rentalRepository.UpdateAsync(rental);
            await UpdateRentalPriceFromAsync(rental, force: request.PriceFrom == null, cancellationToken);
            await UpdateRentalCoverAsync(rental, cancellationToken);

            return await GetRentalAsync(rental.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating rental {rentalId}", request.Id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> PublishRentalAsync(RentalPublishRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var rental = await GetRentalForUpdateAsync(request.RentalId, userContext.TenantId, cancellationToken);
            if (rental == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (!await TenantSupportsRentalsAsync(rental.FkTenantId, cancellationToken))
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Tenant is not enabled for rentals.");

            if (rental.Status == RentalStatus.Published)
                return ApiResponse.Success(true);

            rental.Status = RentalStatus.Published;
            await rentalRepository.UpdateAsync(rental);

            return ApiResponse.Success(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing rental {rentalId}", request.RentalId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> UpdateRentalStatusAsync(RentalStatusUpdateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var rental = await GetRentalForUpdateAsync(request.RentalId, userContext.TenantId, cancellationToken);
            if (rental == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            rental.Status = request.Status;
            await rentalRepository.UpdateAsync(rental);

            return ApiResponse.Success(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating rental status {rentalId}", request.RentalId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> DeleteRentalAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var rental = await GetRentalForUpdateAsync(id, userContext.TenantId, cancellationToken);
            if (rental == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            var deleted = await rentalRepository.DeleteAsync(id);
            return deleted
                ? ApiResponse.Success(true)
                : ApiResponse.Error(ErrorHttp.DbDeleteError);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting rental {rentalId}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
    public async Task<ApiResult> UpsertRentalPricingRulesAsync(
        Guid rentalId,
        List<RentalPricingRuleUpsertRequest> requests,
        bool allOrNothing = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (requests == null || requests.Count == 0)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Request list is empty.");

            var rental = await GetRentalForUpdateAsync(rentalId, userContext.TenantId, cancellationToken);
            if (rental == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            await using IDbContextTransaction? transaction = allOrNothing
                ? await rentalPricingRuleRepository.BeginTransactionAsync()
                : null;

            var results = new List<object>();
            foreach (var request in requests)
            {
                var result = await UpsertRentalPricingRuleAsync(rentalId, request, cancellationToken);
                if (result.Status !=1)
                {
                    if (transaction != null) await transaction.RollbackAsync(cancellationToken);
                    return result;
                }

                results.Add(result);
            }

            if (transaction != null) await transaction.CommitAsync(cancellationToken);

            await UpdateRentalPriceFromAsync(rental, force: true, cancellationToken);

            return ApiResponse.Success(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error upserting pricing rules for rental {RentalId}", rentalId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> UpsertRentalSpecsAsync(
        Guid rentalId,
        List<RentalSpecUpsertRequest> requests,
        bool allOrNothing = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (requests == null || requests.Count == 0)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Request list is empty.");

            var rental = await GetRentalForUpdateAsync(rentalId, userContext.TenantId, cancellationToken);
            if (rental == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            await using IDbContextTransaction? transaction = allOrNothing
                ? await rentalSpecRepository.BeginTransactionAsync()
                : null;

            var results = new List<object>();
            foreach (var request in requests)
            {
                var result = await UpsertRentalSpecAsync(rentalId, request, cancellationToken);
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
            logger.LogError(ex, "Error upserting specs for rental {RentalId}", rentalId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> UpsertRentalPoliciesAsync(
        Guid rentalId,
        List<RentalPolicyUpsertRequest> requests,
        bool allOrNothing = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (requests == null || requests.Count == 0)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Request list is empty.");

            var rental = await GetRentalForUpdateAsync(rentalId, userContext.TenantId, cancellationToken);
            if (rental == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            await using IDbContextTransaction? transaction = allOrNothing
                ? await rentalPolicyRepository.BeginTransactionAsync()
                : null;

            var results = new List<object>();
            foreach (var request in requests)
            {
                var result = await UpsertRentalPolicyAsync(rentalId, request, cancellationToken);
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
            logger.LogError(ex, "Error upserting policies for rental {RentalId}", rentalId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> UpsertRentalMediaAsync(
        Guid rentalId,
        List<RentalMediaUpsertRequest> requests,
        bool allOrNothing = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (requests == null || requests.Count == 0)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Request list is empty.");

            var rental = await GetRentalForUpdateAsync(rentalId, userContext.TenantId, cancellationToken);
            if (rental == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            await using IDbContextTransaction? transaction = allOrNothing
                ? await rentalMediaRepository.BeginTransactionAsync()
                : null;

            var results = new List<object>();
            foreach (var request in requests)
            {
                var result = await UpsertRentalMediaItemAsync(rentalId, request, cancellationToken);
                if (result.Status !=1)
                {
                    if (transaction != null) await transaction.RollbackAsync(cancellationToken);
                    return result;
                }

                results.Add(result);
            }

            if (transaction != null) await transaction.CommitAsync(cancellationToken);

            await UpdateRentalCoverAsync(rental, cancellationToken);

            return ApiResponse.Success(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error upserting media for rental {RentalId}", rentalId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
    private async Task<ApiResult> UpsertRentalPricingRuleAsync(
        Guid rentalId,
        RentalPricingRuleUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var validation = ValidatePricingRuleRequest(request);
        if (validation != null)
            return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, validation);

        if (request.Id.HasValue && request.Id.Value != Guid.Empty)
        {
            var existing = await rentalPricingRuleRepository.GetByIdAsync(request.Id.Value);
            if (existing == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (existing.FkRentalId != rentalId)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Pricing rule does not belong to this rental.");

            existing.Unit = request.Unit;
            existing.PriceAmount = request.PriceAmount;
            existing.Currency = NormalizeCurrency(request.Currency);
            existing.MinQuantity = request.MinQuantity;
            existing.MaxQuantity = request.MaxQuantity;
            existing.IsPrimary = request.IsPrimary;

            var updated = await rentalPricingRuleRepository.UpdateAsync(existing);
            return ApiResponse.Success(RentalConverter.ToDto(updated));
        }

        var created = new RentalPricingRule
        {
            FkRentalId = rentalId,
            Unit = request.Unit,
            PriceAmount = request.PriceAmount,
            Currency = NormalizeCurrency(request.Currency),
            MinQuantity = request.MinQuantity,
            MaxQuantity = request.MaxQuantity,
            IsPrimary = request.IsPrimary,
            Active = true,
            Deleted = false
        };

        var added = await rentalPricingRuleRepository.AddAsync(created);
        return ApiResponse.Success(RentalConverter.ToDto(added));
    }

    private async Task<ApiResult> UpsertRentalSpecAsync(
        Guid rentalId,
        RentalSpecUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var validation = ValidateSpecRequest(request);
        if (validation != null)
            return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, validation);

        if (request.Id.HasValue && request.Id.Value != Guid.Empty)
        {
            var existing = await rentalSpecRepository.GetByIdAsync(request.Id.Value);
            if (existing == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (existing.FkRentalId != rentalId)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Spec does not belong to this rental.");

            existing.Label = request.Label.Trim();
            existing.Value = request.Value.Trim();
            existing.SortOrder = request.SortOrder;

            var updated = await rentalSpecRepository.UpdateAsync(existing);
            return ApiResponse.Success(RentalConverter.ToDto(updated));
        }

        var created = new RentalSpec
        {
            FkRentalId = rentalId,
            Label = request.Label.Trim(),
            Value = request.Value.Trim(),
            SortOrder = request.SortOrder,
            Active = true,
            Deleted = false
        };

        var added = await rentalSpecRepository.AddAsync(created);
        return ApiResponse.Success(RentalConverter.ToDto(added));
    }

    private async Task<ApiResult> UpsertRentalPolicyAsync(
        Guid rentalId,
        RentalPolicyUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var validation = ValidatePolicyRequest(request);
        if (validation != null)
            return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, validation);

        if (request.Id.HasValue && request.Id.Value != Guid.Empty)
        {
            var existing = await rentalPolicyRepository.GetByIdAsync(request.Id.Value);
            if (existing == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (existing.FkRentalId != rentalId)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Policy does not belong to this rental.");

            existing.Title = request.Title.Trim();
            existing.Body = request.Body?.Trim();
            existing.SortOrder = request.SortOrder;

            var updated = await rentalPolicyRepository.UpdateAsync(existing);
            return ApiResponse.Success(RentalConverter.ToDto(updated));
        }

        var created = new RentalPolicy
        {
            FkRentalId = rentalId,
            Title = request.Title.Trim(),
            Body = request.Body?.Trim(),
            SortOrder = request.SortOrder,
            Active = true,
            Deleted = false
        };

        var added = await rentalPolicyRepository.AddAsync(created);
        return ApiResponse.Success(RentalConverter.ToDto(added));
    }

    private async Task<ApiResult> UpsertRentalMediaItemAsync(
        Guid rentalId,
        RentalMediaUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var validation = ValidateMediaRequest(request);
        if (validation != null)
            return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, validation);

        if (request.Id.HasValue && request.Id.Value != Guid.Empty)
        {
            var existing = await rentalMediaRepository.GetByIdAsync(request.Id.Value);
            if (existing == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (existing.FkRentalId != rentalId)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Media does not belong to this rental.");

            existing.FkStoredFileId = request.StoredFileId;
            existing.IsCover = request.IsCover;
            existing.SortOrder = request.SortOrder;

            var updated = await rentalMediaRepository.UpdateAsync(existing);
            return ApiResponse.Success(RentalConverter.ToDto(updated));
        }

        var created = new RentalMedia
        {
            FkRentalId = rentalId,
            FkStoredFileId = request.StoredFileId,
            IsCover = request.IsCover,
            SortOrder = request.SortOrder,
            Active = true,
            Deleted = false
        };

        var added = await rentalMediaRepository.AddAsync(created);
        return ApiResponse.Success(RentalConverter.ToDto(added));
    }
    private async Task CreatePricingRulesAsync(
        Guid rentalId,
        List<RentalPricingRuleUpsertRequest> requests,
        CancellationToken cancellationToken)
    {
        foreach (var request in requests)
        {
            var validation = ValidatePricingRuleRequest(request);
            if (validation != null)
                throw new InvalidOperationException(validation);

            var created = new RentalPricingRule
            {
                FkRentalId = rentalId,
                Unit = request.Unit,
                PriceAmount = request.PriceAmount,
                Currency = NormalizeCurrency(request.Currency),
                MinQuantity = request.MinQuantity,
                MaxQuantity = request.MaxQuantity,
                IsPrimary = request.IsPrimary,
                Active = true,
                Deleted = false
            };

            await rentalPricingRuleRepository.AddAsync(created);
        }
    }

    private async Task CreateSpecsAsync(
        Guid rentalId,
        List<RentalSpecUpsertRequest> requests,
        CancellationToken cancellationToken)
    {
        foreach (var request in requests)
        {
            var validation = ValidateSpecRequest(request);
            if (validation != null)
                throw new InvalidOperationException(validation);

            var created = new RentalSpec
            {
                FkRentalId = rentalId,
                Label = request.Label.Trim(),
                Value = request.Value.Trim(),
                SortOrder = request.SortOrder,
                Active = true,
                Deleted = false
            };

            await rentalSpecRepository.AddAsync(created);
        }
    }

    private async Task CreatePoliciesAsync(
        Guid rentalId,
        List<RentalPolicyUpsertRequest> requests,
        CancellationToken cancellationToken)
    {
        foreach (var request in requests)
        {
            var validation = ValidatePolicyRequest(request);
            if (validation != null)
                throw new InvalidOperationException(validation);

            var created = new RentalPolicy
            {
                FkRentalId = rentalId,
                Title = request.Title.Trim(),
                Body = request.Body?.Trim(),
                SortOrder = request.SortOrder,
                Active = true,
                Deleted = false
            };

            await rentalPolicyRepository.AddAsync(created);
        }
    }

    private async Task CreateMediaAsync(
        Guid rentalId,
        List<RentalMediaUpsertRequest> requests,
        CancellationToken cancellationToken)
    {
        foreach (var request in requests)
        {
            var validation = ValidateMediaRequest(request);
            if (validation != null)
                throw new InvalidOperationException(validation);

            var created = new RentalMedia
            {
                FkRentalId = rentalId,
                FkStoredFileId = request.StoredFileId,
                IsCover = request.IsCover,
                SortOrder = request.SortOrder,
                Active = true,
                Deleted = false
            };

            await rentalMediaRepository.AddAsync(created);
        }
    }
    private async Task<Rental?> GetRentalWithIncludesAsync(
        Guid rentalId,
        Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var query = rentalRepository.Query()
            .AsNoTracking()
            .Include(v => v.FkTenant)
            .Include(v => v.FkCoverMedia)
            .ThenInclude(m => m.FkStoredFile)
            .Where(v =>
                v.Id == rentalId &&
                v.Active &&
                !v.Deleted);

        if (tenantId.HasValue)
            query = query.Where(v => v.FkTenantId == tenantId.Value);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<Rental?> GetRentalForUpdateAsync(
        Guid rentalId,
        Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var query = rentalRepository.Query()
            .Where(v =>
                v.Id == rentalId &&
                v.Active &&
                !v.Deleted);

        if (tenantId.HasValue)
            query = query.Where(v => v.FkTenantId == tenantId.Value);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<bool> TenantSupportsRentalsAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var code = BookingCategoryTypeMapper.ToCode(BookingCategoryType.Spaces);
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

    private async Task UpdateRentalPriceFromAsync(Rental rental, bool force, CancellationToken cancellationToken)
    {
        if (!force && rental.PriceFrom.HasValue)
            return;

        var pricingRules = await rentalPricingRuleRepository.Query()
            .AsNoTracking()
            .Where(r =>
                r.FkRentalId == rental.Id &&
                r.Active &&
                !r.Deleted)
            .ToListAsync(cancellationToken);

        if (pricingRules.Count == 0)
            return;

        var primary = pricingRules.FirstOrDefault(r => r.IsPrimary) ??
                      pricingRules.OrderBy(r => r.PriceAmount).First();

        rental.PriceFrom = primary.PriceAmount;
        rental.Currency = primary.Currency;
        await rentalRepository.UpdateAsync(rental);
    }

    private async Task UpdateRentalCoverAsync(Rental rental, CancellationToken cancellationToken)
    {
        var cover = await rentalMediaRepository.Query()
            .AsNoTracking()
            .Where(m =>
                m.FkRentalId == rental.Id &&
                m.Active &&
                !m.Deleted &&
                m.IsCover)
            .OrderBy(m => m.SortOrder)
            .FirstOrDefaultAsync(cancellationToken);

        if (cover == null)
        {
            cover = await rentalMediaRepository.Query()
                .AsNoTracking()
                .Where(m =>
                    m.FkRentalId == rental.Id &&
                    m.Active &&
                    !m.Deleted)
                .OrderBy(m => m.SortOrder)
                .FirstOrDefaultAsync(cancellationToken);
        }

        rental.FkCoverMediaId = cover?.Id;
        await rentalRepository.UpdateAsync(rental);
    }

    private static RentalMedia? ResolveCover(Rental rental, List<RentalMedia> media)
    {
        if (rental.FkCoverMediaId.HasValue)
            return media.FirstOrDefault(m => m.Id == rental.FkCoverMediaId.Value);

        return media.FirstOrDefault(m => m.IsCover) ?? media.FirstOrDefault();
    }
    private static IQueryable<Rental> ApplySorting(IQueryable<Rental> query, string? sortBy, string? sortDirection)
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
        List<Guid> rentalIds,
        CancellationToken cancellationToken)
    {
        if (rentalIds.Count == 0)
            return new Dictionary<Guid, PriceInfo>();

        var rules = await rentalPricingRuleRepository.Query()
            .AsNoTracking()
            .Where(r =>
                rentalIds.Contains(r.FkRentalId) &&
                r.Active &&
                !r.Deleted)
            .Select(r => new { r.FkRentalId, r.PriceAmount, r.Currency, r.Unit, r.IsPrimary })
            .ToListAsync(cancellationToken);

        return rules
            .GroupBy(r => r.FkRentalId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var primary = g.FirstOrDefault(x => x.IsPrimary) ??
                                  g.OrderBy(x => x.PriceAmount).First();
                    return new PriceInfo(primary.PriceAmount, primary.Currency, primary.Unit);
                });
    }

    private static PriceInfo ResolvePrice(Rental rental, Dictionary<Guid, PriceInfo> lookup)
    {
        if (lookup.TryGetValue(rental.Id, out var info))
            return info;

        return new PriceInfo(rental.PriceFrom, rental.Currency, null);
    }

    private static PriceInfo ResolvePrice(Rental rental, IReadOnlyCollection<RentalPricingRule> rules)
    {
        if (rules.Count == 0)
            return new PriceInfo(rental.PriceFrom, rental.Currency, null);

        var primary = rules.FirstOrDefault(r => r.IsPrimary) ??
                      rules.OrderBy(r => r.PriceAmount).First();

        return new PriceInfo(primary.PriceAmount, primary.Currency, primary.Unit);
    }

    private static string? ValidateRentalRequest(string? title, int unitsAvailable)
    {
        if (string.IsNullOrWhiteSpace(title))
            return "Title is required.";

        if (unitsAvailable < 1)
            return "UnitsAvailable must be at least 1.";

        return null;
    }

    private static string? ValidatePricingRuleRequest(RentalPricingRuleUpsertRequest request)
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

    private static string? ValidateSpecRequest(RentalSpecUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Label))
            return "Spec label is required.";

        if (string.IsNullOrWhiteSpace(request.Value))
            return "Spec value is required.";

        return null;
    }

    private static string? ValidatePolicyRequest(RentalPolicyUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return "Policy title is required.";

        return null;
    }

    private static string? ValidateMediaRequest(RentalMediaUpsertRequest request)
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

    private sealed record PriceInfo(decimal? PriceFrom, string? Currency, RentalPricingUnit? Unit);
}






