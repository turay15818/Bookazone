namespace Bookazone.Application.Interfaces.Repository.Permission;
public interface IPermissionRepository
{
    Task<List<Domain.Entities.Permissions.Permission>> GetAllAsync();
    Task<Domain.Entities.Permissions.Permission?> GetByIdAsync(Guid id);
    Task<Domain.Entities.Permissions.Permission?> GetBySlugAsync(string slug);
    Task<Domain.Entities.Permissions.Permission?> GetByNameAsync(string? name);
    Task<Domain.Entities.Permissions.Permission> CreateAsync(Domain.Entities.Permissions.Permission entity);
    Task<Domain.Entities.Permissions.Permission> UpdateAsync(Domain.Entities.Permissions.Permission entity);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsBySlugAsync(string slug);
}