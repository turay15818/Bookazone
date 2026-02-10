using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;

namespace Bookazone.Domain.Entities.Vehicles;

public class VehicleSpec : UserBaseEntity
{
    public Guid FkVehicleId { get; set; }
    public Vehicle FkVehicle { get; set; } = null!;
    [MaxLength(120)] public string Label { get; set; } = null!;
    [MaxLength(300)] public string Value { get; set; } = null!;
    public int SortOrder { get; set; }
}
