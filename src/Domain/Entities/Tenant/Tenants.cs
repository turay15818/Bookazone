using System.ComponentModel.DataAnnotations;
using Bookazone.Domain.Common;
using Bookazone.Domain.Entities.Profile;
using Bookazone.Domain.Entities.Subscription;

namespace Bookazone.Domain.Entities.Tenant;

public class Tenants : BaseEntity
{
    [MaxLength(100)] public string Name { get; set; } = null!;
    [MaxLength(500)] public string? Description { get; set; }
    [MaxLength(100)] public string? Email { get; set; }
    [MaxLength(20)] public string? Phone { get; set; }
    [MaxLength(200)] public string? Address { get; set; }
    [MaxLength(100)] public string? City { get; set; }
    [MaxLength(100)] public string? State { get; set; }
    [MaxLength(20)] public string? ZipCode { get; set; }
    [MaxLength(100)] public string? Country { get; set; }
    [MaxLength(5000)] public string ? Logo { get; set; }
    [MaxLength(100)] public string? Subdomain { get; set; }
   [MaxLength(4)] public string? Code { get; set; } = null!;
    public bool IsVerified { get; set; } = false;
    
    
    public ICollection<Users> Users { get; set; } = new List<Users>();
    public TenantSettings? TenantSettings { get; set; }
    public ICollection<TenantSubscription> TenantSubscriptions { get; set; } = new List<TenantSubscription>();
}