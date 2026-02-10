using Bookazone.Domain.Common;
using Bookazone.Domain.Entities.Subscription;

namespace Bookazone.Domain.Entities.Tenant;

public class Payment : UserBaseEntity
{
    public Guid FkTenantSubscriptionId { get; set; }
    public TenantSubscription FkTenantSubscription { get; set; } = null!;
}
