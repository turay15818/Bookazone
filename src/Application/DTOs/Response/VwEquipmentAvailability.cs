using Bookazone.Domain.Enums;

namespace Bookazone.Application.DTOs.Response;

public class VwEquipmentAvailability
{
    public Guid EquipmentId { get; set; }
    public DateTime StartLocal { get; set; }
    public DateTime EndLocal { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public EquipmentPricingUnit PricingUnit { get; set; }
    public int UnitsAvailable { get; set; }
    public int UnitsRequested { get; set; }
    public int ReservedUnits { get; set; }
    public bool IsAvailable { get; set; }
    public string? StatusMessage { get; set; }
}
