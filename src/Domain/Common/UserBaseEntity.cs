using Bookazone.Domain.Entities.Profile;

namespace Bookazone.Domain.Common;

public class UserBaseEntity : BaseEntity
{
    public Guid? FkUserId { get; set; }
    public Users? FkUser { get; set; } = null!;
}
