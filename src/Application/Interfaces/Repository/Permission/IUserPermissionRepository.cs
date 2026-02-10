using Bookazone.Domain.Entities.Permissions;

namespace Bookazone.Application.Interfaces.Repository.Permission;

public interface IUserPermissionRepository
{
    List<UserPermission> All();
    List<UserPermission> FindByUserId(Guid? userId);
    UserPermission? Find(Guid? id);
    UserPermission? Create(UserPermission entity);
    UserPermission? Update(UserPermission entity);
    bool Delete(Guid? id);
    bool Status(Guid? id, bool active);
    
    Task<bool> SyncUserPermissionsAsync(Guid userId, List<Guid> permissionIds, string? updatedBy = null);
    Task<bool> SyncUserPermissionsByNamesAsync(Guid userId, List<string>? permissionNames, string? updatedBy = null);
}