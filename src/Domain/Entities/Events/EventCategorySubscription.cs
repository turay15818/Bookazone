using Bookazone.Domain.Common;

namespace Bookazone.Domain.Entities.Events;

public class EventCategorySubscription : UserBaseEntity
{
    public Guid FkEventCategoryId { get; set; }
    public EventCategory FkEventCategory { get; set; } = null!;
    public bool ReceivePush { get; set; } = true;
    public bool ReceiveEmail { get; set; } = true;
}
