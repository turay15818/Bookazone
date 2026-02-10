namespace Bookazone.Application.DTOs.Response;

public class VwSportResourceBlackout
{
    public Guid Id { get; set; }
    public Guid ResourceId { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public string? Reason { get; set; }
}
