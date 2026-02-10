using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Domain.Entities.Permissions;
using Bookazone.Domain.Entities.Profile;


namespace Bookazone.Application.DTOs.Converter;

public abstract class UserConverter
{
    public static UserDto ToDto(Users entity, List<UserPermission>? userPermissions = null)
    {
        return new UserDto
        {
            Id = entity.Id,
            Username = entity.Username ?? "-",
            FirstName = entity.Firstname ?? "-",
            LastName = entity.Lastname ?? "-",
            Email = entity.Email ?? "-",
            Phone = entity.Phone ?? "-",
            Address = entity.Address,
            Birthdate = entity.Birthdate,
            Gender = entity.Gender,
            ProfileImage = entity.FkTenant?.Logo != null ? $"{entity.FkTenant.Logo}" : null,
            LastAuthenticated = entity.LastAuthenticated?.ToString("G"),
            EmailVerified = entity.EmailVerified,
            PhoneVerified = entity.PhoneVerified,
            Frozen = entity.Frozen,
            PermissionUserManagement = userPermissions?.FirstOrDefault(a => a.FkPermission?.Name == Consts.Roles.TenantAdmin) != null
        };
    }

    public static Users ToEntity(UserDto entity)
    {
        return new Users
        {
            Id = entity.Id,
            Username = entity.Username,
            Firstname = entity.FirstName,
            Lastname = entity.LastName,
            Email = entity.Email,
            Phone = entity.Phone,
            ProfileImage = entity.ProfileImage != null ? $"{entity.ProfileImage}" : null,
        };
    }
}