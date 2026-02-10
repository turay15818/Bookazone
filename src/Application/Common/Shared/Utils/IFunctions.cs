using Bookazone.Domain.Entities.Profile;

namespace Bookazone.Application.Common.Shared.Utils;

public interface IFunctions
{
    Users? GetUser(string? username);
    IEnumerable<DateTime> EachDay(DateTime from, DateTime thru);
}