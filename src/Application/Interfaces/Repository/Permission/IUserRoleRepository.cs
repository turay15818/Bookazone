using Bookazone.Domain.Entities.Permissions;

namespace Bookazone.Application.Interfaces.Repository.Permission;

public interface IUserRoleRepository
{
    // Basic CRUD
    List<UserRole> All();
    UserRole? Find(Guid? id);
    List<UserRole> FindByUserId(Guid? userId);
    UserRole? Create(UserRole entity);
    UserRole? Update(UserRole entity);
    bool Delete(Guid? id);
    bool Status(Guid? id, bool active);

    // Async
    Task<List<UserRole>> AllAsync();
    Task<UserRole?> FindAsync(Guid? id);
    Task<UserRole?> CreateAsync(UserRole entity);
    Task<UserRole?> UpdateAsync(UserRole entity);
    Task<bool> DeleteAsync(Guid? id);
    Task<bool> StatusAsync(Guid? id, bool active);

    // Relationship management
    // Task<List<Guid>> GetRoleIdsByUserAsync(Guid userId);
    Task<bool> AssignRolesToUserAsync(Guid userId, List<Guid> roleIds, string? updatedBy = null);
    Task<bool> RemoveRolesFromUserAsync(Guid userId, List<Guid>? roleIds = null);
    Task<List<Role>> GetRolesByUserAsync(Guid userId);
    Task<bool> UserHasRoleAsync(Guid userId, string roleName);
}