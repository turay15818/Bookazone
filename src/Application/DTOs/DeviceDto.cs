using System.ComponentModel.DataAnnotations;

namespace Bookazone.Application.DTOs;

public class DeviceDto
{
    public Guid? Id { get; set; }

    [MaxLength(150)]
    public string? Slug { get; set; }

    [MaxLength(1000)]
    public string? Name { get; set; }

    [MaxLength(1000)]
    public string? Identifier { get; set; }

    [MaxLength(150)]
    public string? Version { get; set; }

    [MaxLength(100)]
    public string? AppVersion { get; set; }

    [MaxLength(1000)]
    public string? System { get; set; }

    public bool? Verified { get; set; }
    public bool? Biometric { get; set; }

    public DateTime? DateVerified { get; set; }
    public DateTime? Lastconnect { get; set; }

    public UserDto? UserDto { get; set; }
}