using System.ComponentModel.DataAnnotations;

namespace Bookazone.Application.DTOs;

public class ShopVm
{
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
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = null!;
    public string? TenantLogo { get; set; }
    public string? TenantSubdomain { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? DateCreated { get; set; }
    public DateTime? DateUpdated { get; set; }
}

public class ShopCreateRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = null!;
    [MaxLength(200)] public string? Address { get; set; }
    [MaxLength(100)] public string? City { get; set; }
    [MaxLength(100)] public string? State { get; set; }
    [MaxLength(20)] public string? ZipCode { get; set; }
    [MaxLength(100)] public string? Country { get; set; }
    [MaxLength(20)] public string? Phone { get; set; }
    public bool IsMainShop { get; set; } = false;
    public Guid? DefaultWarehouseId { get; set; }
}

public class ShopUpdateRequest
{
    public Guid Id { get; set; }
    [MaxLength(100)] public string? Name { get; set; }
    [MaxLength(200)] public string? Address { get; set; }
    [MaxLength(100)] public string? City { get; set; }
    [MaxLength(100)] public string? State { get; set; }
    [MaxLength(20)] public string? ZipCode { get; set; }
    [MaxLength(100)] public string? Country { get; set; }
    [MaxLength(20)] public string? Phone { get; set; }
    public bool? IsMainShop { get; set; }
    public Guid? DefaultWarehouseId { get; set; }
    public bool? Active { get; set; }
}