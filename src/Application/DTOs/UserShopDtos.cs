using System.ComponentModel.DataAnnotations;

namespace Bookazone.Application.DTOs;

public class UserShopAssignRequest
{
    [Required]
    public Guid UserId { get; set; }
    [Required]
    public Guid ShopId { get; set; }
}

public class UserShopVm
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string? Username { get; set; }
    public Guid ShopId { get; set; }
    public string? ShopName { get; set; }
    public Guid TenantId { get; set; }
    public DateTime? DateCreated { get; set; }
    public bool Active { get; set; }
}