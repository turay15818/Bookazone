using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;

namespace Bookazone.Domain.Entities.Rentals;

public class RentalSpec : UserBaseEntity
{
    public Guid FkRentalId { get; set; }
    public Rental FkRental { get; set; } = null!;
    [MaxLength(100)] public string Label { get; set; } = null!;
    [MaxLength(300)] public string Value { get; set; } = null!;
    public int SortOrder { get; set; }
}
