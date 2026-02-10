using Microsoft.EntityFrameworkCore.Storage;
using Bookazone.Application.DTOs;
using Bookazone.Domain.Entities.Profile;

namespace Bookazone.Application.Interfaces.Repository.Profile;

public interface IUserRepository 
{
    List<Users>? All(Users? user);
    (List<UserDto> Items, int Total, int Pages) AllByTenantDto(Guid tenantId, int page, int pageSize, string? q, Guid? shopId = null);
    UserDto? GetDetailsDto(Guid userId);

    Users? Find(Guid? id);
    Users? FindByShop(Guid? shopId);
    Task<Users>? GetTenantIdAsync(Guid? id);
    List<Users>? FindUncompletedRegistration(string? username = null, string? email = null, string? phone = null);
    List<Users>? FindByEmailOrPhoneOrUsername(string? username);
    Task<Users>? CreateAsync(Users entity);
    Users? Update(Users entity);
    bool Delete(Guid? id);
    bool Status(Guid? id, bool active);
    bool ExistsByEmailOrPhoneOrUsername(string? username);
    
    Task<IDbContextTransaction> BeginTransactionAsync();
}