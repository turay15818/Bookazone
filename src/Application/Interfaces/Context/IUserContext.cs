namespace Bookazone.Application.Interfaces.Context;

public interface IUserContext
{
    string? Username { get; } 
    Guid? UserId { get; } 
    Guid? TenantId { get; }  
    string? TenantCode { get; } 
    Guid? ShopId { get; }
}

