using Bookazone.Application.DTOs;
using Bookazone.Domain.Entities.Profile;

namespace Bookazone.Application.Interfaces.Repository.Security;

public interface IUserAccessService
{
    Task<(bool rolesChanged, bool permissionsChanged)> UpdateUserAccessAsync(
        Users userToUpdate,
        List<string>? newRoles,
        List<string>? newPermissions,
        string? updatedBy
    );

    Task<(Users user, bool rolesChanged, bool permissionsChanged)> CreateOrUpdateUserWithAccessAsync(
        Users userEntity,
        List<string>? roleNames,
        List<string>? permissionNames,
        string? createdBy
    );
    Task<UserAccessDto?> GetUserAccessAsync(Guid userId);
}