using Bookazone.Domain.Entities.Profile;

namespace Bookazone.Application.Interfaces.Repository.Profile;

public interface IUserPasswordRepository
{
    Task<UserPassword?> GetCurrentPasswordAsync(Guid userId);
    Task<IEnumerable<UserPassword>> GetPasswordHistoryAsync(Guid userId, int limit = 5);
    Task<bool> ValidatePasswordAsync(Guid userId, string plainPassword);
    Task<UserPassword> CreatePasswordAsync(Guid userId, string plainPassword, string changedBy);
    Task<bool> MarkPasswordAsUsedAsync(Guid userId);
    Task<bool> IsPasswordReusedAsync(Guid userId, string plainPassword, int historyLimit = 5);
}
