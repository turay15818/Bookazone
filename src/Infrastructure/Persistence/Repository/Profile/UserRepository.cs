using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs;
using Bookazone.Application.Interfaces.Repository.Profile;
using Bookazone.Domain.Entities.Profile;
using Bookazone.Infrastructure.Persistence.DbContext;
using Functions = Bookazone.Application.Common.Shared.Utils.Functions;

namespace Bookazone.Infrastructure.Persistence.Repository.Profile;

public class UserRepository(BookazoneDbContext bookazoneDbContext, ILogger<UserRepository> logger) : IUserRepository
{
    public List<Users>? All(Users? user)
    {
        return user == null
            ? null
            : (bookazoneDbContext.Users ?? throw new Except(ErrorHttp.DbQueryRunFailed))
            .Where(a => a.Id == user.Id && (a.Deleted == false && a.Active ==true)).ToListAsync().Result;
    }
    
    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await bookazoneDbContext.Database.BeginTransactionAsync();
    }
    

    public (List<UserDto> Items, int Total, int Pages) AllByTenantDto(Guid tenantId, int page, int pageSize, string? q, Guid? shopId = null)
    {
        if (bookazoneDbContext.Users == null) throw new Except(ErrorHttp.DbQueryRunFailed);

        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var query = bookazoneDbContext.Users
            .AsNoTracking()
            .Where(u => u.FkTenantId == tenantId && u.Active && !u.Deleted);
        /*if (shopId != null && shopId != Guid.Empty)
        {
            query = query
                .Where(u => u.UserShops.Any(us => us.FkShopId == shopId && us.Active && !us.Deleted));
            Console.WriteLine("Filtering users by shop ID: " + shopId);
        }*/
        if (!string.IsNullOrWhiteSpace(q))
        {
            var ql = q.Trim().ToLower();
            query = query.Where(u =>
                ((u.Username ?? "").ToLower().Contains(ql)) ||
                ((u.Firstname ?? "").ToLower().Contains(ql)) ||
                ((u.Lastname ?? "").ToLower().Contains(ql)) ||
                ((u.Email ?? "").ToLower().Contains(ql)) ||
                ((u.Phone ?? "").ToLower().Contains(ql)) ||
                ((u.Address ?? "").ToLower().Contains(ql))
            );
        }

        var total = query.Count();
        var pages = (int)Math.Ceiling(total / (double)pageSize);
        var skip = (page - 1) * pageSize;

        var users = query
            .OrderByDescending(u => u.DateCreated)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync().Result;

        var items = BuildUserDtos(users);
        return (items, total, pages);
    }

    private List<UserDto> BuildUserDtos(List<Users> users)
    {
        if (users.Count == 0) return new List<UserDto>();

        var userIds = users.Select(u => u.Id).ToList();

        // Load roles for these users
        var userRoles = bookazoneDbContext.UserRoles!
            .AsNoTracking()
            .Include(ur => ur.FkRole)
            .Where(ur => userIds.Contains(ur.FkUserId) && !ur.Deleted && ur.Active && ur.FkRole != null && ur.FkRole.Active && !ur.FkRole.Deleted)
            .ToListAsync().Result;

        var rolesByUser = userRoles
            .GroupBy(ur => ur.FkUserId)
            .ToDictionary(g => g.Key, g => g.Select(ur => ur.FkRole!.Name).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct(StringComparer.OrdinalIgnoreCase).ToList());

        // Load direct user permissions
        var directPerms = bookazoneDbContext.UserPermissions!
            .AsNoTracking()
            .Include(up => up.FkPermission)
            .Where(up => userIds.Contains(up.FkUserId) && !up.Deleted && up.Active && up.FkPermission != null && up.FkPermission.Active && !up.FkPermission.Deleted)
            .ToListAsync().Result;

        var directPermsByUser = directPerms
            .GroupBy(up => up.FkUserId)
            .ToDictionary(g => g.Key, g => g.Select(up => up.FkPermission!.Name).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct(StringComparer.OrdinalIgnoreCase).ToList());

        // Load role->permission mappings and expand
        var roleIds = userRoles.Select(ur => ur.FkRoleId).Distinct().ToList();
        var rolePerms = roleIds.Count == 0
            ? new List<Domain.Entities.Permissions.RolePermission>()
            : bookazoneDbContext.RolePermissions!
                .AsNoTracking()
                .Include(rp => rp.FkPermission)
                .Where(rp => roleIds.Contains(rp.FkRoleId) && !rp.Deleted && rp.Active && rp.FkPermission != null && rp.FkPermission.Active && !rp.FkPermission.Deleted)
                .ToListAsync().Result;

        var permNamesByRoleId = rolePerms
            .GroupBy(rp => rp.FkRoleId)
            .ToDictionary(g => g.Key, g => g.Select(rp => rp.FkPermission!.Name).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct(StringComparer.OrdinalIgnoreCase).ToList());

        // Build final DTOs
        var result = new List<UserDto>(users.Count);
        foreach (var u in users)
        {
            var roles = rolesByUser.TryGetValue(u.Id, out var rList) ? rList : new List<string>();

            // Merge direct permissions with those from roles
            var permSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (directPermsByUser.TryGetValue(u.Id, out var dpList))
            {
                foreach (var p in dpList) permSet.Add(p);
            }
            foreach (var ur in userRoles.Where(x => x.FkUserId == u.Id))
            {
                if (permNamesByRoleId.TryGetValue(ur.FkRoleId, out var rpList))
                {
                    foreach (var p in rpList) permSet.Add(p);
                }
            }

            var dto = new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Phone = u.Phone,
                FirstName = u.Firstname,
                LastName = u.Lastname,
                Gender = u.Gender,
                ReferralId = null,
                Address = u.Address,
                ProfileImage = u.ProfileImage,
                District = null,
                Birthdate = u.Birthdate,
                EmailVerified = u.EmailVerified,
                PhoneVerified = u.PhoneVerified,
                Frozen = u.Frozen,
                PermissionUserManagement = roles.Contains(Consts.Roles.UserManagement),
                LastAuthenticated = u.LastAuthenticated != null ? u.LastAuthenticated.Value.ToString("G") : null,
                Roles = roles,
                Permissions = permSet.ToList()
            };

            result.Add(dto);
        }

        return result;
    }

    public Users? Find(Guid? id)
    {
        if (bookazoneDbContext.Users == null) throw new Except(ErrorHttp.DbQueryRunFailed);
        return bookazoneDbContext.Users.FindAsync(id).Result;
    }
    
    public Users? FindByShop(Guid? shopsId)
    {
        if (bookazoneDbContext.Users == null) throw new Except(ErrorHttp.DbQueryRunFailed);
        return bookazoneDbContext.Users
          //  .Include(u => u.UserShops)
            .FirstOrDefault();
           // .FirstOrDefault(u => u.UserShops.Any(us => us.FkShopId == shopsId && !us.Deleted && us.Active));
    }

    public async Task<Users>? GetTenantIdAsync(Guid? id)
    {
        if (bookazoneDbContext.Users == null) throw new Except(ErrorHttp.DbQueryRunFailed);
        return bookazoneDbContext.Users.FindAsync(id).Result!;
    }
    public List<Users>? FindUncompletedRegistration(string? username = null, string? email = null, string? phone = null)
    {
        if (username != null && email != null && phone != null) return null;
        var data = (bookazoneDbContext.Users ?? throw new Except(ErrorHttp.DbQueryRunFailed))
            .Where(a => a.Active == true && a.Deleted != true);

        if (username != null) data = data.Where(a => a.Username == username);
        if (email != null) data = data.Where(a => a.Email == email);
        if (phone != null) data = data.Where(a => a.Phone == phone);
        return data.ToListAsync().Result;
    }

    public bool ExistsByEmailOrPhoneOrUsername(string? usernname)
    {
        if (string.IsNullOrWhiteSpace(usernname) || bookazoneDbContext.Users == null)
            return false;
        if (new EmailAddressAttribute().IsValid(usernname))
        {
            return bookazoneDbContext.Users.Any(u =>
                (u.Email == usernname || u.Username == usernname) && u.Active == true);
        }

        var formattedPhone = Functions.PhoneLocalFormat(usernname);
        var existsByPhone = !string.IsNullOrWhiteSpace(formattedPhone) &&
                            bookazoneDbContext.Users.Any(u =>
                                u.Phone == formattedPhone && u.Active == true);
        var existsByUsername = bookazoneDbContext.Users.Any(u =>
            u.Username == usernname && u.Active == true);
        return existsByPhone || existsByUsername;
    }

    public List<Users>? FindByEmailOrPhoneOrUsername(string? username)
    {
        if (bookazoneDbContext.Users == null) throw new Except(ErrorHttp.DbQueryRunFailed);
        if (new EmailAddressAttribute().IsValid(username))
            return username != null
                ? bookazoneDbContext.Users
                    .Where(a => (a.Email == username || a.Username == username) && a.Active == true && a.Email != null)
                    .ToListAsync().Result
                : null;
        var phone = Functions.PhoneLocalFormat(username);
        if (username == null && phone == null)
            return null;
        var users = bookazoneDbContext.Users.Where(a => a.Phone == phone && a.Active == true && a.Phone != null)
            .ToListAsync().Result;
        if (users.Count == 0)
            users = bookazoneDbContext.Users.Where(a => a.Username == username && a.Active == true).ToListAsync()
                .Result;
        return users;
    }


    public async Task <Users>? CreateAsync(Users entity)
    {
        entity.DateCreated = DateTime.UtcNow;
        entity.Active = true;
        entity.Deleted = false;
        var checkForPhone = FindByEmailOrPhoneOrUsername(entity.Phone);
        var checkForEmail = FindByEmailOrPhoneOrUsername(entity.Email);
        var checkForUsername = FindByEmailOrPhoneOrUsername(entity.Username);
        if (checkForPhone != null && checkForPhone.Count > 0) throw new Except(ErrorHttp.UserPhoneAlreadyExists);
        if (checkForEmail != null && checkForEmail.Count > 0) throw new Except(ErrorHttp.UserEmailAlreadyExists);
        if (checkForUsername != null && checkForUsername.Count > 0)
            throw new Except(ErrorHttp.UserUsernameAlreadyExists);
        if (bookazoneDbContext.Users == null) throw new Except(ErrorHttp.DbCreateError);
        bookazoneDbContext.Users.Add(entity);
        var result = bookazoneDbContext.SaveChangesAsync().Result;
        return result > 0 ? entity : null;
    }

    public Users? Update(Users entity)
    {
        entity.DateUpdated = DateTime.UtcNow;
        if (bookazoneDbContext.Users == null) throw new Except(ErrorHttp.DbCreateError);
        bookazoneDbContext.Users.Update(entity);
        var result = bookazoneDbContext.SaveChangesAsync().Result;
        return result > 0 ? entity : null;
    }

    public bool Delete(Guid? id)
    {
        var entity = Find(id);
        if (id == null || entity == null) throw new Except(ErrorHttp.NotFound);
        entity.Active = false;
        entity.Deleted = true;
        entity.DateDeleted = DateTime.UtcNow;
        if (bookazoneDbContext.Users == null) throw new Except(ErrorHttp.DbCreateError);
        bookazoneDbContext.Users.Update(entity);
        var result = bookazoneDbContext.SaveChangesAsync().Result;
        return result > 0;
    }

    public bool Status(Guid? id, bool active)
    {
        var entity = Find(id);
        if (id == null || entity == null) throw new Except(ErrorHttp.NotFound);
        entity.Active = active;
        entity.DateUpdated = DateTime.UtcNow;
        if (bookazoneDbContext.Users == null) throw new Except(ErrorHttp.DbCreateError);
        bookazoneDbContext.Users.Update(entity);
        var result = bookazoneDbContext.SaveChangesAsync().Result;
        return result > 0;
    }

    public UserDto? GetDetailsDto(Guid userId)
    {
        if (bookazoneDbContext.Users == null) throw new Except(ErrorHttp.DbQueryRunFailed);

        var u = bookazoneDbContext.Users
            .AsNoTracking()
            .FirstOrDefault(x => x.Id == userId && !x.Deleted);
        if (u == null) return null;

        // Roles for this user
        var userRoles = bookazoneDbContext.UserRoles!
            .AsNoTracking()
            .Include(ur => ur.FkRole)
            .Where(ur => ur.FkUserId == u.Id && !ur.Deleted && ur.Active && ur.FkRole != null && ur.FkRole.Active && !ur.FkRole.Deleted)
            .ToList();
        var roles = userRoles
            .Select(ur => ur.FkRole!.Name)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Direct permissions
        var directPerms = bookazoneDbContext.UserPermissions!
            .AsNoTracking()
            .Include(up => up.FkPermission)
            .Where(up => up.FkUserId == u.Id && !up.Deleted && up.Active && up.FkPermission != null && up.FkPermission.Active && !up.FkPermission.Deleted)
            .ToList();

        // Permissions via roles
        var roleIds = userRoles.Select(ur => ur.FkRoleId).Distinct().ToList();
        var rolePerms = roleIds.Count == 0
            ? new List<Domain.Entities.Permissions.RolePermission>()
            : bookazoneDbContext.RolePermissions!
                .AsNoTracking()
                .Include(rp => rp.FkPermission)
                .Where(rp => roleIds.Contains(rp.FkRoleId) && !rp.Deleted && rp.Active && rp.FkPermission != null && rp.FkPermission.Active && !rp.FkPermission.Deleted)
                .ToList();

        var permSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in directPerms)
        {
            if (!string.IsNullOrWhiteSpace(p.FkPermission?.Name)) permSet.Add(p.FkPermission!.Name);
        }
        foreach (var rp in rolePerms)
        {
            if (!string.IsNullOrWhiteSpace(rp.FkPermission?.Name)) permSet.Add(rp.FkPermission!.Name);
        }

        var dto = new UserDto
        {
            Id = u.Id,
            Username = u.Username,
            Email = u.Email,
            Phone = u.Phone,
            FirstName = u.Firstname,
            LastName = u.Lastname,
            Gender = u.Gender,
            Address = u.Address,
            ProfileImage = u.ProfileImage,
            Birthdate = u.Birthdate,
            EmailVerified = u.EmailVerified,
            PhoneVerified = u.PhoneVerified,
            Frozen = u.Frozen,
            LastAuthenticated = u.LastAuthenticated?.ToString("G"),
            Roles = roles,
            Permissions = permSet.Distinct(StringComparer.OrdinalIgnoreCase).ToList()
        };
        dto.PermissionUserManagement = (dto.Roles.Contains(Consts.Roles.UserManagement));
        return dto;
    }
}
