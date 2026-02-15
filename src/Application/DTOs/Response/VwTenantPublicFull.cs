using Bookazone.Application.DTOs;

namespace Bookazone.Application.DTOs.Response;

public class VwTenantPublicDetail
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }
    public string? Logo { get; set; }
    public string? Subdomain { get; set; }
    public string? Code { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? DateCreated { get; set; }
}



public class VwTenantFullPublic
{
    public VwTenantPublicDetail Tenant { get; set; } = new();
    public List<VwTenantBookingCategory> Categories { get; set; } = new();
    public List<VwTenantWorkingHour> WorkingHours { get; set; } = new();
    public VwTenantSettings? Settings { get; set; }
}