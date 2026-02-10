using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookazone.Api.Controllers.V1.Config;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.Interfaces.Repository.Subscription;
using Bookazone.Application.Interfaces.Repository.Tenant;
using Bookazone.Domain.Entities.Subscription;
using Bookazone.Infrastructure.Services;

namespace Bookazone.Api.Controllers.V1.Secure.Subscription;

[ApiController]
[Route(RouteWebVersion1.Secure.TenantSubscription.Base)]
public class TenantSubscriptionController(
    ITenantSubscriptionRepository tenantSubscriptionRepository,
    ITenantRepository tenantRepository,
    ISubscriptionPlanRepository subscriptionPlanRepository,
    ILogger<TenantSubscriptionController> logger,
    SubscriptionEmailService emailService,
    IFunctions functions
) : ControllerBase
{
    [HttpGet]
    [Authorize]
    [Route(RouteWebVersion1.Secure.TenantSubscription.All)]
    public async Task<ApiResult> GetCurrentSubscription()
    {
        try
        {
            var user = functions.GetUser(User.Identity?.Name);
            if (user == null)
                return ApiResponse.Error(ErrorHttp.TokenExpired);

            if (user.FkTenantId == null)
                return ApiResponse.Error(ErrorHttp.Unauthorized, new Exception("No tenant associated."));

            var current = await tenantSubscriptionRepository.GetActiveForTenantAsync(user.FkTenantId.Value);
            return current == null
                ? ApiResponse.Error(ErrorHttp.NotFound, new Exception("No active subscription found."))
                : ApiResponse.Success(current);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching tenant subscription");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpGet]
    [Authorize]
    [Route(RouteWebVersion1.Secure.TenantSubscription.History)]
    public async Task<ApiResult> GetSubscriptionHistory()
    {
        try
        {
            var user = functions.GetUser(User.Identity?.Name);
            if (user == null)
                return ApiResponse.Error(ErrorHttp.TokenExpired);

            if (user.FkTenantId == null)
                return ApiResponse.Error(ErrorHttp.Unauthorized, new Exception("No tenant associated."));

            var history = await tenantSubscriptionRepository.ListByTenantAsync(user.FkTenantId.Value);
            return ApiResponse.Success(history);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching subscription history");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpPost]
    [Authorize]
    [Route(RouteWebVersion1.Secure.TenantSubscription.Upgrade)]
    
public async Task<ApiResult> Upgrade([FromBody] TenantUpgradeRequest request)
{
    try
    {
        var user = functions.GetUser(User.Identity?.Name);
        if (user == null)
            return ApiResponse.Error(ErrorHttp.TokenExpired);
        if (user.FkTenantId == null)
            return ApiResponse.Error(ErrorHttp.Unauthorized, new Exception("No tenant associated."));
        var tenant = await tenantRepository.GetByIdAsync(user.FkTenantId.Value);
        if (tenant == null)
            return ApiResponse.Error(ErrorHttp.NotFound, new Exception("Tenant not found."));
        var plan = await subscriptionPlanRepository.FindAsync(request.PlanId);
        if (plan == null)
            return ApiResponse.Error(ErrorHttp.NotFound, new Exception("Selected plan not found."));
        var current = await tenantSubscriptionRepository.GetActiveForTenantAsync(tenant.Id);
        if (current != null)
            await tenantSubscriptionRepository.DeactivateAsync(current.Id, user.Username);
        var newSubscription = new TenantSubscription
        {
            FkTenantId = tenant.Id,
            FkPlanId = request.PlanId,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(1),
            IsActive = true,
            IsTrial = false,
            CreatedBy = user.Username,
            DateCreated = DateTime.UtcNow
        };

        await tenantSubscriptionRepository.CreateAsync(newSubscription);
        await emailService.SendSubscriptionCreatedEmailAsync(tenant, new SubscriptionPlan
        {
            Name = plan.Name,
            Description = plan.Description,
            PricePerPeriod = plan.PricePerPeriod,
            Period = plan.Period
        });

        return ApiResponse.Success(new
        {
            message = $"Tenant upgraded to plan {plan.Name} successfully.",
            plan = plan
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error upgrading tenant subscription");
        return ApiResponse.Error(ErrorHttp.Error, ex);
    }
}
    
}

public class TenantUpgradeRequest
{
    public Guid PlanId { get; set; }
}
