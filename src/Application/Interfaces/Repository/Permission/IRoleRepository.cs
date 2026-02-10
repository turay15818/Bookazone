using Bookazone.Domain.Entities.Permissions;

namespace Bookazone.Application.Interfaces.Repository.Permission;

public interface IRoleRepository
{
    Task<List<Role>> AllAsync();
    Task<Role?> FindByNameAsync(string name);
    Task<Role?> FindAsync(Guid? id);
    Task<Role?> CreateAsync(Role entity);
    Task<Role?> UpdateAsync(Role entity);
    Task<bool> DeleteAsync(Guid? id);
    Task<bool> StatusAsync(Guid? id, bool active);

    // Role-Permission management
    Task<bool> AssignPermissionsAsync(Guid roleId, List<Guid> permissionIds, string? updatedBy = null);

    Task<bool> UpdateRolePermissionsAsync(Guid roleId, List<Guid>? providedPermissions, string? updatedBy = null);
    Task<List<Domain.Entities.Permissions.Permission>> GetPermissionsByRoleAsync(Guid roleId);
    Task<bool> ClearPermissionsAsync(Guid roleId);
    Task<List<object>> GetAllRolesWithPermissionsAsync();
}