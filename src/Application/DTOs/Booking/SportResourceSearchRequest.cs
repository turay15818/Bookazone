namespace Bookazone.Application.DTOs.Booking;

public class SportResourceSearchRequest
{
    public Guid? TenantId { get; set; }
    public Guid? SportTypeId { get; set; }
    public int? MinCapacity { get; set; }
    public string? Search { get; set; }
    public string? City { get; set; }
}
