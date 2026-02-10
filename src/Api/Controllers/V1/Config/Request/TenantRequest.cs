using System.ComponentModel.DataAnnotations;

namespace Bookazone.Api.Controllers.V1.Config.Request;

public class TenantRequest : BaseRequest
{
    [MaxLength(100)] public string Name { get; set; } = null!;
    [MaxLength(100)] public string? Description { get; set; }
    [MaxLength(100)] public string? Email { get; set; }
    [MaxLength(20)] public string? Phone { get; set; }
    [MaxLength(200)] public string? Address { get; set; }
    [MaxLength(100)] public string? City { get; set; }
    [MaxLength(100)] public string? State { get; set; }
    [MaxLength(20)] public string? ZipCode { get; set; }
    [MaxLength(100)] public string? Country { get; set; }
    public IFormFile? Logo { get; set; }

    public TenantUserRequest? AdminUser { get; set; }
}

public class TenantUpdateRequest : BaseRequest
{
    [MaxLength(100)] public string Name { get; set; } = null!;
    [MaxLength(100)] public string? Description { get; set; }
    [MaxLength(100)] public string? Email { get; set; }
    [MaxLength(20)] public string? Phone { get; set; }
    [MaxLength(200)] public string? Address { get; set; }
    [MaxLength(100)] public string? City { get; set; }
    [MaxLength(100)] public string? State { get; set; }
    [MaxLength(20)] public string? ZipCode { get; set; }
    [MaxLength(100)] public string? Country { get; set; }
    public IFormFile? Logo { get; set; }
}


public class TenantUserRequest
{
    public string? Firstname { get; set; }
    public string? Lastname { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}


public class VerifyAccountRequest
{
    public string Token { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public VerifyDeviceRequest Device { get; set; } = new();
}

public class VerifyDeviceRequest
{
    public string Identifier { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Os { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string? AppVersion { get; set; }
}
