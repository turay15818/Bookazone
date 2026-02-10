using System.ComponentModel.DataAnnotations;

namespace Bookazone.Application.DTOs;

public class TenantBriefVm
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? Logo { get; set; }
    public string? Subdomain { get; set; }
    public bool IsVerified { get; set; }
    public DateTime? DateCreated { get; set; }
}

public class TenantShopBriefVm
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public bool IsMainShop { get; set; }
    public bool Active { get; set; }
    public DateTime? DateCreated { get; set; }
}

public class TenantUserBriefVm
{
    public Guid Id { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool Active { get; set; }
    public DateTime? LastAuthenticated { get; set; }
}

public class TenantDetailsVm
{
    public TenantBriefVm Tenant { get; set; } = null!;

    // Aggregates
    public int UserCount { get; set; }
    public int ShopCount { get; set; }

    // Collections
    public List<TenantShopBriefVm> Shops { get; set; } = new();
    public List<TenantUserBriefVm> Users { get; set; } = new();
}

public class ShopUserVm
{
    public Guid? Id { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool Active { get; set; }
    public DateTime? DateCreated { get; set; }
}

public class ShopDetailsVm
{
    // Shop data
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }
    public string? Phone { get; set; }
    public bool IsMainShop { get; set; }
    public bool Active { get; set; }
    public DateTime? DateCreated { get; set; }

    // Tenant brief
    public TenantBriefVm Tenant { get; set; } = null!;

    // Users under shop
    public List<ShopUserVm> Users { get; set; } = new();
}