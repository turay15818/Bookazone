using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;

namespace Bookazone.Domain.Entities.Others;

public class AuditLog : BaseEntity
{
    [MaxLength(150)] public string? Name { get; set; }
    [MaxLength(150)] public string? IpAddress { get; set; }
    [MaxLength(10000)] public string? Url { get; set; }
    [MaxLength(10000)] public string? QueryString { get; set; }
    [MaxLength(Int32.MaxValue)]public string? Payload { get; set; }
    [MaxLength(Int32.MaxValue)]public string? Response { get; set; }
    [MaxLength(Int32.MaxValue)]public string? RequestHeaders { get; set; }
    [MaxLength(Int32.MaxValue)]public string? RequestContentType { get; set; }
    [MaxLength(Int32.MaxValue)]public string? RequestMethod { get; set; }
    [MaxLength(Int32.MaxValue)] public string? ResponseStatusCode { get; set; }
    [MaxLength(Int32.MaxValue)]public string? ResponseHeaders { get; set; }
    [MaxLength(Int32.MaxValue)]public string? ResponseContentType { get; set; }
    public DateTime? ResponseTimestamp { get; set; }
    [MaxLength(1000)] public string? User { get; set; }
}