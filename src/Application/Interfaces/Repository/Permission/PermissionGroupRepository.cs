using Bookazone.Domain.Entities.Permissions;

namespace Bookazone.Application.Interfaces.Repository.Permission;

public interface IPermissionGroupRepository
{
    // Sync
    List<PermissionGroup> All();
    PermissionGroup? Find(Guid? id);
    PermissionGroup? FindByName(string? name);
    PermissionGroup? Create(PermissionGroup entity);
    PermissionGroup? Update(PermissionGroup entity);
    bool Delete(Guid? id);
    bool Status(Guid? id, bool active);

    // Async
    Task<List<PermissionGroup>> AllAsync();
    Task<PermissionGroup?> FindAsync(Guid? id);
    Task<PermissionGroup?> CreateAsync(PermissionGroup entity);
    Task<PermissionGroup?> UpdateAsync(PermissionGroup entity);
    Task<bool> DeleteAsync(Guid? id);
    Task<bool> StatusAsync(Guid? id, bool active);
}