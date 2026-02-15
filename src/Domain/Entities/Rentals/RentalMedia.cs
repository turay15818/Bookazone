using Bookazone.Domain.Common;
using Bookazone.Domain.Entities.Others;

namespace Bookazone.Domain.Entities.Rentals;

public class RentalMedia : UserBaseEntity
{
    public Guid FkRentalId { get; set; }
    public Rental FkRental { get; set; } = null!;
    public Guid FkStoredFileId { get; set; }
    public StoredFile FkStoredFile { get; set; } = null!;
    public bool IsCover { get; set; } = false;
    public int SortOrder { get; set; }
}
