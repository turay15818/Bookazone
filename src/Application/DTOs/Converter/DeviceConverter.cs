using Bookazone.Domain.Entities.Profile;

namespace Bookazone.Application.DTOs.Converter;

public abstract class DeviceConverter
{
    public static DeviceDto ToDto(Device entity)
    {
        return new DeviceDto
        {
            Id = entity.Id,
            Slug = entity.Slug,
            Name = entity.Name,
            Identifier = entity.Identifier,
            Version = entity.Version,
            AppVersion = entity.AppVersion,
            System = entity.System,
            Verified = entity.Verified,
            Biometric = entity.Biometric,
            DateVerified = entity.DateVerified,
            Lastconnect = entity.LastConnect,
            UserDto = entity.FkUser != null ? UserConverter.ToDto(entity.FkUser) : null
        };
    }

    public static Device ToEntity(DeviceDto entity)
    {
        return new Device
        {
            Id = entity.Id ?? Guid.NewGuid(),
            Slug = entity.Slug,
            Name = entity.Name,
            Identifier = entity.Identifier,
            Version = entity.Version,
            AppVersion = entity.AppVersion,
            System = entity.System,
            Verified = entity.Verified,
            Biometric = entity.Biometric,
            DateVerified = entity.DateVerified,
        };
    }
}