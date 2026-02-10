using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;

namespace Bookazone.Domain.Entities.Vehicles;

public class VehiclePolicy : UserBaseEntity
{
    public Guid FkVehicleId { get; set; }
    public Vehicle FkVehicle { get; set; } = null!;
    [MaxLength(120)] public string Title { get; set; } = null!;
    [MaxLength(2000)] public string? Body { get; set; }
    public int SortOrder { get; set; }
}
