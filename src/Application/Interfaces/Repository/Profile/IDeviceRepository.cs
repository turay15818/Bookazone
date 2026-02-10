using Bookazone.Domain.Entities.Profile;

namespace Bookazone.Application.Interfaces.Repository.Profile;

public interface IDeviceRepository
{
    List<Device>? All(Users? user);
    List<Device> AllActive();
    Device? Find(Guid? id);
    Device? FindByIdentifier(string? identifier);
    Device? FindByIdAndUser(Guid? id, Users? customer);
    List<Device>? ListByIdentifierAndUser(string? identifier, Users? user);
    List<Device>? ListByUser(Users? user);
    List<Device> GetUserDevicesWithTokens(Users userId);

    Device? FindByIdentifierAndUser(string? identifier, Users? user);
    Device? FindByToken(string? token);
    Device? Create(Device account);
    Device? Update(Device account);
    bool Delete(Guid? id);
    bool Status(Guid? id, bool active);


    Task<Device?> GetByIdentifierAsync(string identifier, Guid userId);
    Task<Device?> CreateAsync(Device entity);
}