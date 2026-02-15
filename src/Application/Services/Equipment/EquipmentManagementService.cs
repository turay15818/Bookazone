using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs;
using Bookazone.Application.DTOs.Converter;
using Bookazone.Application.DTOs.Equipment;
using Bookazone.Application.DTOs.Response;
using Bookazone.Application.Interfaces.Context;
using Bookazone.Application.Interfaces.Repository.Booking;
using Bookazone.Application.Interfaces.Repository.Equipment;
using Bookazone.Application.Interfaces.Repository.Tenant;
using Bookazone.Application.Interfaces.Services.Equipment;
using Bookazone.Domain.Entities.Equipment;
using EquipmentEntity = Bookazone.Domain.Entities.Equipment.Equipment;
using Bookazone.Domain.Enums;

namespace Bookazone.Application.Services.Equipment;

public class EquipmentManagementService(
    IEquipmentRepository equipmentRepository,
    IEquipmentPricingRuleRepository equipmentPricingRuleRepository,
    IEquipmentSpecRepository equipmentSpecRepository,
    IEquipmentPolicyRepository equipmentPolicyRepository,
    IEquipmentMediaRepository equipmentMediaRepository,
    ITenantRepository tenantRepository,
    ITenantBookingCategoryRepository tenantBookingCategoryRepository,
    IUserContext userContext,
    IHttpContextAccessor httpContextAccessor,
    ILogger<EquipmentManagementService> logger)
    : IEquipmentManagementService
{
    public async Task<ApiResult> GetTenantEquipmentAsync(
        List<EquipmentStatus>? statuses = null,
        string? category = null,
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

            var query = equipmentRepository.Query()
                .AsNoTracking()
                .Include(e => e.FkTenant)
                .Include(e => e.FkCoverMedia)
                .ThenInclude(m => m.FkStoredFile)
                .Where(e =>
                    e.FkTenantId == tenantId &&
                    e.Active &&
                    !e.Deleted);

            if (statuses is { Count: > 0 })
                query = query.Where(e => statuses.Contains(e.Status));

            if (!string.IsNullOrWhiteSpace(category))
            {
                var categoryTerm = category.Trim().ToLowerInvariant();
                query = query.Where(e => e.Category != null && e.Category.ToLower().Contains(categoryTerm));
            }

            if (!string.IsNullOrWhiteSpace(city))
            {
                var cityTerm = city.Trim().ToLowerInvariant();
                query = query.Where(e => e.City != null && e.City.ToLower().Contains(cityTerm));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLowerInvariant();
                query = query.Where(e =>
                    e.Title.ToLower().Contains(term) ||
                    (e.Subtitle != null && e.Subtitle.ToLower().Contains(term)) ||
                    (e.Description != null && e.Description.ToLower().Contains(term)) ||
                    (e.Category != null && e.Category.ToLower().Contains(term)) ||
                    (e.Address != null && e.Address.ToLower().Contains(term)) ||
                    (e.City != null && e.City.ToLower().Contains(term)) ||
                    e.FkTenant.Name.ToLower().Contains(term));
            }
            var sorted = ApplySorting(query, sortBy, sortDirection);
            var paged = await Paginate<EquipmentEntity>.CreateAsync(sorted, pageIndex, pageSize);

            var pageData = paged.Data ?? new List<EquipmentEntity>();
            var equipmentIds = pageData.Select(e => e.Id).ToList();
            var priceLookup = await BuildPriceLookupAsync(equipmentIds, cancellationToken);

            var dto = pageData.Select(e =>
            {
                var priceInfo = ResolvePrice(e, priceLookup);
                var card = EquipmentConverter.ToListItemDto(
                    e,
                    e.FkTenant,
                    e.FkCoverMedia,
                    priceInfo.PriceFrom,
                    priceInfo.Currency,
                    priceInfo.Unit);
                card.CoverUrl = FileUrlHelper.BuildPublicUrl(
                    httpContextAccessor.HttpContext?.Request,
                    card.CoverUrl);
                return card;
            }).ToList();

            var response = new DataPaginate<VwEquipmentListItem>
            {
                Data = dto,
                Pages = paged.Pages,
                PageIndex = paged.PageIndex
            };

            return ApiResponse.Success(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting tenant equipment");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
    public async Task<ApiResult> GetEquipmentAsync(Guid equipmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var equipment = await GetEquipmentWithIncludesAsync(equipmentId, userContext.TenantId, cancellationToken);
            if (equipment == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            var pricingRules = await equipmentPricingRuleRepository.GetByEquipmentAsync(equipmentId, cancellationToken);
            var specs = await equipmentSpecRepository.GetByEquipmentAsync(equipmentId, cancellationToken);
            var policies = await equipmentPolicyRepository.GetByEquipmentAsync(equipmentId, cancellationToken);
            var media = await equipmentMediaRepository.GetByEquipmentAsync(equipmentId, cancellationToken);

            var priceInfo = ResolvePrice(equipment, pricingRules);
            var coverMedia = ResolveCover(equipment, media);

            var mediaDto = media.Select(EquipmentConverter.ToDto).ToList();
            foreach (var item in mediaDto)
                item.Url = FileUrlHelper.BuildPublicUrl(
                               httpContextAccessor.HttpContext?.Request,
                               item.Url) ?? string.Empty;

            var detail = EquipmentConverter.ToDetailDto(
                equipment,
                equipment.FkTenant,
                coverMedia,
                pricingRules.Select(EquipmentConverter.ToDto),
                specs.Select(EquipmentConverter.ToDto),
                policies.Select(EquipmentConverter.ToDto),
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
            logger.LogError(ex, "Error getting equipment {EquipmentId}", equipmentId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
    public async Task<ApiResult> CreateEquipmentAsync(EquipmentCreateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var validation = ValidateEquipmentRequest(request.Title, request.UnitsAvailable);
            if (validation != null)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, validation);

            if (request.TenantId == Guid.Empty)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "TenantId is required.");

            if (IsTenantMismatch(request.TenantId))
                return ApiResponse.Error(ErrorHttp.Forbidden, new Exception("User is not authorized for this tenant."));

            if (!await tenantRepository.ExistsAsync(request.TenantId))
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (!await TenantSupportsEquipmentAsync(request.TenantId, cancellationToken))
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Tenant is not enabled for equipment.");

            var equipment = new EquipmentEntity
            {
                FkTenantId = request.TenantId,
                Title = request.Title.Trim(),
                Subtitle = request.Subtitle?.Trim(),
                Description = request.Description?.Trim(),
                Category = request.Category?.Trim(),
                City = request.City?.Trim(),
                Address = request.Address?.Trim(),
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                UnitsAvailable = request.UnitsAvailable,
                DeliveryAvailable = request.DeliveryAvailable,
                PickupAvailable = request.PickupAvailable,
                MinBookingDuration = request.MinBookingDuration,
                MaxBookingDuration = request.MaxBookingDuration,
                BookingDurationUnit = request.BookingDurationUnit,
                BookingMode = request.BookingMode,
                Status = EquipmentStatus.Draft,
                PriceFrom = request.PriceFrom,
                Currency = NormalizeCurrency(request.Currency),
                Active = true,
                Deleted = false
            };

            await using var transaction = await equipmentRepository.BeginTransactionAsync();
            var created = await equipmentRepository.AddAsync(equipment);
            if (request.PricingRules is { Count: > 0 })
                await CreatePricingRulesAsync(created.Id, request.PricingRules, cancellationToken);

            if (request.Specs is { Count: > 0 })
                await CreateSpecsAsync(created.Id, request.Specs, cancellationToken);

            if (request.Policies is { Count: > 0 })
                await CreatePoliciesAsync(created.Id, request.Policies, cancellationToken);

            if (request.Media is { Count: > 0 })
                await CreateMediaAsync(created.Id, request.Media, cancellationToken);

            await UpdateEquipmentPriceFromAsync(created, force: request.PriceFrom == null, cancellationToken);
            await UpdateEquipmentCoverAsync(created, cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return await GetEquipmentAsync(created.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating equipment for tenant {TenantId}", request.TenantId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
    public async Task<ApiResult> UpdateEquipmentAsync(EquipmentUpdateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var validation = ValidateEquipmentRequest(request.Title, request.UnitsAvailable);
            if (validation != null)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, validation);

            if (request.TenantId == Guid.Empty)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "TenantId is required.");

            if (IsTenantMismatch(request.TenantId))
                return ApiResponse.Error(ErrorHttp.Forbidden, new Exception("User is not authorized for this tenant."));

            var equipment = await GetEquipmentForUpdateAsync(request.Id, request.TenantId, cancellationToken);
            if (equipment == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            equipment.Title = request.Title.Trim();
            equipment.Subtitle = request.Subtitle?.Trim();
            equipment.Description = request.Description?.Trim();
            equipment.Category = request.Category?.Trim();
            equipment.City = request.City?.Trim();
            equipment.Address = request.Address?.Trim();
            equipment.Latitude = request.Latitude;
            equipment.Longitude = request.Longitude;
            equipment.UnitsAvailable = request.UnitsAvailable;
            equipment.DeliveryAvailable = request.DeliveryAvailable;
            equipment.PickupAvailable = request.PickupAvailable;
            equipment.MinBookingDuration = request.MinBookingDuration;
            equipment.MaxBookingDuration = request.MaxBookingDuration;
            equipment.BookingDurationUnit = request.BookingDurationUnit;
            equipment.BookingMode = request.BookingMode;
            equipment.PriceFrom = request.PriceFrom;
            equipment.Currency = NormalizeCurrency(request.Currency);

            await equipmentRepository.UpdateAsync(equipment);
            await UpdateEquipmentPriceFromAsync(equipment, force: request.PriceFrom == null, cancellationToken);
            await UpdateEquipmentCoverAsync(equipment, cancellationToken);

            return await GetEquipmentAsync(equipment.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating equipment {EquipmentId}", request.Id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
    public async Task<ApiResult> PublishEquipmentAsync(EquipmentPublishRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var equipment = await GetEquipmentForUpdateAsync(request.EquipmentId, userContext.TenantId, cancellationToken);
            if (equipment == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (!await TenantSupportsEquipmentAsync(equipment.FkTenantId, cancellationToken))
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Tenant is not enabled for equipment.");

            if (equipment.Status == EquipmentStatus.Published)
                return ApiResponse.Success(true);

            equipment.Status = EquipmentStatus.Published;
            await equipmentRepository.UpdateAsync(equipment);

            return ApiResponse.Success(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error publishing equipment {EquipmentId}", request.EquipmentId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> UpdateEquipmentStatusAsync(EquipmentStatusUpdateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var equipment = await GetEquipmentForUpdateAsync(request.EquipmentId, userContext.TenantId, cancellationToken);
            if (equipment == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            equipment.Status = request.Status;
            await equipmentRepository.UpdateAsync(equipment);

            return ApiResponse.Success(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating equipment status {EquipmentId}", request.EquipmentId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> DeleteEquipmentAsync(Guid equipmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var equipment = await GetEquipmentForUpdateAsync(equipmentId, userContext.TenantId, cancellationToken);
            if (equipment == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            var deleted = await equipmentRepository.DeleteAsync(equipmentId);
            return deleted
                ? ApiResponse.Success(true)
                : ApiResponse.Error(ErrorHttp.DbDeleteError);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting equipment {EquipmentId}", equipmentId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
    public async Task<ApiResult> UpsertEquipmentPricingRulesAsync(
        Guid equipmentId,
        List<EquipmentPricingRuleUpsertRequest> requests,
        bool allOrNothing = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (requests == null || requests.Count == 0)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Request list is empty.");

            var equipment = await GetEquipmentForUpdateAsync(equipmentId, userContext.TenantId, cancellationToken);
            if (equipment == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            await using IDbContextTransaction? transaction = allOrNothing
                ? await equipmentPricingRuleRepository.BeginTransactionAsync()
                : null;

            var results = new List<object>();
            foreach (var request in requests)
            {
                var result = await UpsertEquipmentPricingRuleAsync(equipmentId, request, cancellationToken);
                if (result.Status != 1)
                {
                    if (transaction != null) await transaction.RollbackAsync(cancellationToken);
                    return result;
                }

                results.Add(result);
            }

            if (transaction != null) await transaction.CommitAsync(cancellationToken);

            await UpdateEquipmentPriceFromAsync(equipment, force: true, cancellationToken);

            return ApiResponse.Success(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error upserting pricing rules for equipment {EquipmentId}", equipmentId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
    public async Task<ApiResult> UpsertEquipmentSpecsAsync(
        Guid equipmentId,
        List<EquipmentSpecUpsertRequest> requests,
        bool allOrNothing = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (requests == null || requests.Count == 0)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Request list is empty.");

            var equipment = await GetEquipmentForUpdateAsync(equipmentId, userContext.TenantId, cancellationToken);
            logger.LogError("The request fail here");
            if (equipment == null)
                return ApiResponse.Error(ErrorHttp.NotFound);
            logger.LogError("The request reach here");
            await using IDbContextTransaction? transaction = allOrNothing
                ? await equipmentSpecRepository.BeginTransactionAsync()
                : null;

            var results = new List<object>();
            foreach (var request in requests)
            {
                var result = await UpsertEquipmentSpecAsync(equipmentId, request, cancellationToken);
                if (result.Status != 1)
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
            logger.LogError(ex, "Error upserting specs for equipment {EquipmentId}", equipmentId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
    public async Task<ApiResult> UpsertEquipmentPoliciesAsync(
        Guid equipmentId,
        List<EquipmentPolicyUpsertRequest> requests,
        bool allOrNothing = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (requests == null || requests.Count == 0)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Request list is empty.");

            var equipment = await GetEquipmentForUpdateAsync(equipmentId, userContext.TenantId, cancellationToken);
            if (equipment == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            await using IDbContextTransaction? transaction = allOrNothing
                ? await equipmentPolicyRepository.BeginTransactionAsync()
                : null;

            var results = new List<object>();
            foreach (var request in requests)
            {
                var result = await UpsertEquipmentPolicyAsync(equipmentId, request, cancellationToken);
                if (result.Status != 1)
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
            logger.LogError(ex, "Error upserting policies for equipment {EquipmentId}", equipmentId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> UpsertEquipmentMediaAsync(
        Guid equipmentId,
        List<EquipmentMediaUpsertRequest> requests,
        bool allOrNothing = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (requests == null || requests.Count == 0)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Request list is empty.");

            var equipment = await GetEquipmentForUpdateAsync(equipmentId, userContext.TenantId, cancellationToken);
            if (equipment == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            await using IDbContextTransaction? transaction = allOrNothing
                ? await equipmentMediaRepository.BeginTransactionAsync()
                : null;

            var results = new List<object>();
            foreach (var request in requests)
            {
                var result = await UpsertEquipmentMediaItemAsync(equipmentId, request, cancellationToken);
                if (result.Status != 1)
                {
                    if (transaction != null) await transaction.RollbackAsync(cancellationToken);
                    return result;
                }

                results.Add(result);
            }

            if (transaction != null) await transaction.CommitAsync(cancellationToken);

            await UpdateEquipmentCoverAsync(equipment, cancellationToken);

            return ApiResponse.Success(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error upserting media for equipment {EquipmentId}", equipmentId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
    private async Task<ApiResult> UpsertEquipmentPricingRuleAsync(
        Guid equipmentId,
        EquipmentPricingRuleUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var validation = ValidatePricingRuleRequest(request);
        if (validation != null)
            return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, validation);

        if (request.Id.HasValue && request.Id.Value != Guid.Empty)
        {
            var existing = await equipmentPricingRuleRepository.GetByIdAsync(request.Id.Value);
            if (existing == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (existing.FkEquipmentId != equipmentId)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Pricing rule does not belong to this equipment.");

            existing.Unit = request.Unit;
            existing.PriceAmount = request.PriceAmount;
            existing.Currency = NormalizeCurrency(request.Currency);
            existing.MinQuantity = request.MinQuantity;
            existing.MaxQuantity = request.MaxQuantity;
            existing.IsPrimary = request.IsPrimary;

            var updated = await equipmentPricingRuleRepository.UpdateAsync(existing);
            return ApiResponse.Success(EquipmentConverter.ToDto(updated));
        }

        var created = new EquipmentPricingRule
        {
            FkEquipmentId = equipmentId,
            Unit = request.Unit,
            PriceAmount = request.PriceAmount,
            Currency = NormalizeCurrency(request.Currency),
            MinQuantity = request.MinQuantity,
            MaxQuantity = request.MaxQuantity,
            IsPrimary = request.IsPrimary,
            Active = true,
            Deleted = false
        };

        var added = await equipmentPricingRuleRepository.AddAsync(created);
        return ApiResponse.Success(EquipmentConverter.ToDto(added));
    }
    private async Task<ApiResult> UpsertEquipmentSpecAsync(
        Guid equipmentId,
        EquipmentSpecUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var validation = ValidateSpecRequest(request);
        if (validation != null)
            return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, validation);

        if (request.Id.HasValue && request.Id.Value != Guid.Empty)
        {
            var existing = await equipmentSpecRepository.GetByIdAsync(request.Id.Value);
            if (existing == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (existing.FkEquipmentId != equipmentId)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Spec does not belong to this equipment.");

            existing.Label = request.Label.Trim();
            existing.Value = request.Value.Trim();
            existing.SortOrder = request.SortOrder;

            var updated = await equipmentSpecRepository.UpdateAsync(existing);
            return ApiResponse.Success(EquipmentConverter.ToDto(updated));
        }

        var created = new EquipmentSpec
        {
            FkEquipmentId = equipmentId,
            Label = request.Label.Trim(),
            Value = request.Value.Trim(),
            SortOrder = request.SortOrder,
            Active = true,
            Deleted = false
        };

        var added = await equipmentSpecRepository.AddAsync(created);
        return ApiResponse.Success(EquipmentConverter.ToDto(added));
    }
    private async Task<ApiResult> UpsertEquipmentPolicyAsync(
        Guid equipmentId,
        EquipmentPolicyUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var validation = ValidatePolicyRequest(request);
        if (validation != null)
            return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, validation);

        if (request.Id.HasValue && request.Id.Value != Guid.Empty)
        {
            var existing = await equipmentPolicyRepository.GetByIdAsync(request.Id.Value);
            if (existing == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (existing.FkEquipmentId != equipmentId)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Policy does not belong to this equipment.");

            existing.Title = request.Title.Trim();
            existing.Body = request.Body?.Trim();
            existing.SortOrder = request.SortOrder;

            var updated = await equipmentPolicyRepository.UpdateAsync(existing);
            return ApiResponse.Success(EquipmentConverter.ToDto(updated));
        }

        var created = new EquipmentPolicy
        {
            FkEquipmentId = equipmentId,
            Title = request.Title.Trim(),
            Body = request.Body?.Trim(),
            SortOrder = request.SortOrder,
            Active = true,
            Deleted = false
        };

        var added = await equipmentPolicyRepository.AddAsync(created);
        return ApiResponse.Success(EquipmentConverter.ToDto(added));
    }

    private async Task<ApiResult> UpsertEquipmentMediaItemAsync(
        Guid equipmentId,
        EquipmentMediaUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var validation = ValidateMediaRequest(request);
        if (validation != null)
            return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, validation);

        if (request.Id.HasValue && request.Id.Value != Guid.Empty)
        {
            var existing = await equipmentMediaRepository.GetByIdAsync(request.Id.Value);
            if (existing == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (existing.FkEquipmentId != equipmentId)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Media does not belong to this equipment.");

            existing.FkStoredFileId = request.StoredFileId;
            existing.IsCover = request.IsCover;
            existing.SortOrder = request.SortOrder;

            var updated = await equipmentMediaRepository.UpdateAsync(existing);
            return ApiResponse.Success(EquipmentConverter.ToDto(updated));
        }

        var created = new EquipmentMedia
        {
            FkEquipmentId = equipmentId,
            FkStoredFileId = request.StoredFileId,
            IsCover = request.IsCover,
            SortOrder = request.SortOrder,
            Active = true,
            Deleted = false
        };

        var added = await equipmentMediaRepository.AddAsync(created);
        return ApiResponse.Success(EquipmentConverter.ToDto(added));
    }
    private async Task CreatePricingRulesAsync(
        Guid equipmentId,
        List<EquipmentPricingRuleUpsertRequest> requests,
        CancellationToken cancellationToken)
    {
        foreach (var request in requests)
        {
            var validation = ValidatePricingRuleRequest(request);
            if (validation != null)
                throw new InvalidOperationException(validation);

            var created = new EquipmentPricingRule
            {
                FkEquipmentId = equipmentId,
                Unit = request.Unit,
                PriceAmount = request.PriceAmount,
                Currency = NormalizeCurrency(request.Currency),
                MinQuantity = request.MinQuantity,
                MaxQuantity = request.MaxQuantity,
                IsPrimary = request.IsPrimary,
                Active = true,
                Deleted = false
            };

            await equipmentPricingRuleRepository.AddAsync(created);
        }
    }

    private async Task CreateSpecsAsync(
        Guid equipmentId,
        List<EquipmentSpecUpsertRequest> requests,
        CancellationToken cancellationToken)
    {
        foreach (var request in requests)
        {
            var validation = ValidateSpecRequest(request);
            if (validation != null)
                throw new InvalidOperationException(validation);

            var created = new EquipmentSpec
            {
                FkEquipmentId = equipmentId,
                Label = request.Label.Trim(),
                Value = request.Value.Trim(),
                SortOrder = request.SortOrder,
                Active = true,
                Deleted = false
            };

            await equipmentSpecRepository.AddAsync(created);
        }
    }

    private async Task CreatePoliciesAsync(
        Guid equipmentId,
        List<EquipmentPolicyUpsertRequest> requests,
        CancellationToken cancellationToken)
    {
        foreach (var request in requests)
        {
            var validation = ValidatePolicyRequest(request);
            if (validation != null)
                throw new InvalidOperationException(validation);

            var created = new EquipmentPolicy
            {
                FkEquipmentId = equipmentId,
                Title = request.Title.Trim(),
                Body = request.Body?.Trim(),
                SortOrder = request.SortOrder,
                Active = true,
                Deleted = false
            };

            await equipmentPolicyRepository.AddAsync(created);
        }
    }

    private async Task CreateMediaAsync(
        Guid equipmentId,
        List<EquipmentMediaUpsertRequest> requests,
        CancellationToken cancellationToken)
    {
        foreach (var request in requests)
        {
            var validation = ValidateMediaRequest(request);
            if (validation != null)
                throw new InvalidOperationException(validation);

            var created = new EquipmentMedia
            {
                FkEquipmentId = equipmentId,
                FkStoredFileId = request.StoredFileId,
                IsCover = request.IsCover,
                SortOrder = request.SortOrder,
                Active = true,
                Deleted = false
            };

            await equipmentMediaRepository.AddAsync(created);
        }
    }

    private async Task<EquipmentEntity?> GetEquipmentWithIncludesAsync(
        Guid equipmentId,
        Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var query = equipmentRepository.Query()
            .AsNoTracking()
            .Include(e => e.FkTenant)
            .Include(e => e.FkCoverMedia)
            .ThenInclude(m => m.FkStoredFile)
            .Where(e =>
                e.Id == equipmentId &&
                e.Active &&
                !e.Deleted);

        if (tenantId.HasValue)
            query = query.Where(e => e.FkTenantId == tenantId.Value);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
    private async Task<EquipmentEntity?> GetEquipmentForUpdateAsync(
        Guid equipmentId,
        Guid? tenantId,
        CancellationToken cancellationToken)
    {
        var query = equipmentRepository.Query()
            .Where(e =>
                e.Id == equipmentId &&
                e.Active &&
                !e.Deleted);

        if (tenantId.HasValue)
            query = query.Where(e => e.FkTenantId == tenantId.Value);

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
    private async Task<bool> TenantSupportsEquipmentAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var code = BookingCategoryTypeMapper.ToCode(BookingCategoryType.EquipmentRental);
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
    private async Task UpdateEquipmentPriceFromAsync(EquipmentEntity equipment, bool force, CancellationToken cancellationToken)
    {
        if (!force && equipment.PriceFrom.HasValue)
            return;

        var pricingRules = await equipmentPricingRuleRepository.Query()
            .AsNoTracking()
            .Where(r =>
                r.FkEquipmentId == equipment.Id &&
                r.Active &&
                !r.Deleted)
            .ToListAsync(cancellationToken);

        if (pricingRules.Count == 0)
            return;

        var primary = pricingRules.FirstOrDefault(r => r.IsPrimary) ??
                      pricingRules.OrderBy(r => r.PriceAmount).First();

        equipment.PriceFrom = primary.PriceAmount;
        equipment.Currency = primary.Currency;
        await equipmentRepository.UpdateAsync(equipment);
    }
    private async Task UpdateEquipmentCoverAsync(EquipmentEntity equipment, CancellationToken cancellationToken)
    {
        var cover = await equipmentMediaRepository.Query()
            .AsNoTracking()
            .Where(m =>
                m.FkEquipmentId == equipment.Id &&
                m.Active &&
                !m.Deleted &&
                m.IsCover)
            .OrderBy(m => m.SortOrder)
            .FirstOrDefaultAsync(cancellationToken);

        if (cover == null)
        {
            cover = await equipmentMediaRepository.Query()
                .AsNoTracking()
                .Where(m =>
                    m.FkEquipmentId == equipment.Id &&
                    m.Active &&
                    !m.Deleted)
                .OrderBy(m => m.SortOrder)
                .FirstOrDefaultAsync(cancellationToken);
        }

        equipment.FkCoverMediaId = cover?.Id;
        await equipmentRepository.UpdateAsync(equipment);
    }
    private static EquipmentMedia? ResolveCover(EquipmentEntity equipment, List<EquipmentMedia> media)
    {
        if (equipment.FkCoverMediaId.HasValue)
            return media.FirstOrDefault(m => m.Id == equipment.FkCoverMediaId.Value);

        return media.FirstOrDefault(m => m.IsCover) ?? media.FirstOrDefault();
    }

    private static IQueryable<EquipmentEntity> ApplySorting(IQueryable<EquipmentEntity> query, string? sortBy, string? sortDirection)
    {
        var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        return (sortBy ?? string.Empty).Trim().ToLowerInvariant() switch
        {
            "title" => isDesc ? query.OrderByDescending(e => e.Title) : query.OrderBy(e => e.Title),
            "price" => isDesc ? query.OrderByDescending(e => e.PriceFrom) : query.OrderBy(e => e.PriceFrom),
            "status" => isDesc ? query.OrderByDescending(e => e.Status) : query.OrderBy(e => e.Status),
            "city" => isDesc ? query.OrderByDescending(e => e.City) : query.OrderBy(e => e.City),
            "category" => isDesc ? query.OrderByDescending(e => e.Category) : query.OrderBy(e => e.Category),
            _ => isDesc ? query.OrderByDescending(e => e.DateCreated) : query.OrderBy(e => e.DateCreated)
        };
    }
    private async Task<Dictionary<Guid, PriceInfo>> BuildPriceLookupAsync(
        List<Guid> equipmentIds,
        CancellationToken cancellationToken)
    {
        if (equipmentIds.Count == 0)
            return new Dictionary<Guid, PriceInfo>();

        var rules = await equipmentPricingRuleRepository.Query()
            .AsNoTracking()
            .Where(r =>
                equipmentIds.Contains(r.FkEquipmentId) &&
                r.Active &&
                !r.Deleted)
            .Select(r => new { r.FkEquipmentId, r.PriceAmount, r.Currency, r.Unit, r.IsPrimary })
            .ToListAsync(cancellationToken);

        return rules
            .GroupBy(r => r.FkEquipmentId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var primary = g.FirstOrDefault(x => x.IsPrimary) ??
                                  g.OrderBy(x => x.PriceAmount).First();
                    return new PriceInfo(primary.PriceAmount, primary.Currency, primary.Unit);
                });
    }

    private static PriceInfo ResolvePrice(EquipmentEntity equipment, Dictionary<Guid, PriceInfo> lookup)
    {
        if (lookup.TryGetValue(equipment.Id, out var info))
            return info;

        return new PriceInfo(equipment.PriceFrom, equipment.Currency, null);
    }

    private static PriceInfo ResolvePrice(EquipmentEntity equipment, IReadOnlyCollection<EquipmentPricingRule> rules)
    {
        if (rules.Count == 0)
            return new PriceInfo(equipment.PriceFrom, equipment.Currency, null);

        var primary = rules.FirstOrDefault(r => r.IsPrimary) ??
                      rules.OrderBy(r => r.PriceAmount).First();

        return new PriceInfo(primary.PriceAmount, primary.Currency, primary.Unit);
    }
    private static string? ValidateEquipmentRequest(string? title, int unitsAvailable)
    {
        if (string.IsNullOrWhiteSpace(title))
            return "Title is required.";

        if (unitsAvailable < 1)
            return "UnitsAvailable must be at least 1.";

        return null;
    }

    private static string? ValidatePricingRuleRequest(EquipmentPricingRuleUpsertRequest request)
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

    private static string? ValidateSpecRequest(EquipmentSpecUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Label))
            return "Spec label is required.";

        if (string.IsNullOrWhiteSpace(request.Value))
            return "Spec value is required.";

        return null;
    }

    private static string? ValidatePolicyRequest(EquipmentPolicyUpsertRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return "Policy title is required.";

        return null;
    }

    private static string? ValidateMediaRequest(EquipmentMediaUpsertRequest request)
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

    private sealed record PriceInfo(decimal? PriceFrom, string? Currency, EquipmentPricingUnit? Unit);
}