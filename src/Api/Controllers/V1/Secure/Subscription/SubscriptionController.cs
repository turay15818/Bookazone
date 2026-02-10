using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookazone.Api.Controllers.V1.Config;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.Interfaces.Repository.Subscription;
using Bookazone.Domain.Entities.Subscription;

namespace Bookazone.Api.Controllers.V1.Secure.Subscription;

[ApiController]
[Route(RouteWebVersion1.Secure.Subscription.Base)]
public class SubscriptionController(
    ISubscriptionPlanRepository subscriptionPlanRepository,
    ILogger<SubscriptionController> logger
) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [Route(RouteWebVersion1.Secure.Subscription.All)]
    public async Task<ApiResult> GetAll()
    {
        try
        {
            var plans = await subscriptionPlanRepository.AllAsync();
            return ApiResponse.Success(plans);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching subscription plans");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    [HttpGet]
    [AllowAnonymous]
    [Route(RouteWebVersion1.Secure.Subscription.Find)]
    public async Task<ApiResult> GetById(Guid id)
    {
        try
        {
            var plan = await subscriptionPlanRepository.FindAsync(id);
            if (plan == null)
                return ApiResponse.Error(ErrorHttp.NotFound, new Exception("Plan not found."));
            return ApiResponse.Success(plan);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching plan details for {Id}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
    
    [HttpPost]
    [Authorize(Roles = Consts.Permissions.PlatformSubscriptionsManage)]
    [Route(RouteWebVersion1.Secure.Subscription.Create)]
    public async Task<ApiResult> Create([FromBody] SubscriptionPlan plan)
    {
        try
        {
            var created = await subscriptionPlanRepository.CreateAsync(plan);
            return ApiResponse.Success(created);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating subscription plan");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
    
    [HttpPut]
    [Authorize(Roles = Consts.Permissions.PlatformSubscriptionsManage)]
    [Route(RouteWebVersion1.Secure.Subscription.Delete)]
    public async Task<ApiResult> Update(Guid id, [FromBody] SubscriptionPlan plan)
    {
        try
        {
            plan.Id = id;
            var updated = await subscriptionPlanRepository.UpdateAsync(plan);
            return ApiResponse.Success(updated);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating subscription plan {Id}", id);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }
}
