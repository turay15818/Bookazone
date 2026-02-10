using Bookazone.Domain.Enums;

namespace Bookazone.Application.Common.Shared.Utils;

public static class BookingCategoryTypeMapper
{
    public static string ToCode(BookingCategoryType type)
    {
        return type switch
        {
            BookingCategoryType.Sports => "SPORTS",
            BookingCategoryType.Events => "EVENTS",
            BookingCategoryType.Vehicles => "VEHICLES",
            BookingCategoryType.Spaces => "SPACES",
            BookingCategoryType.Salons => "SALONS",
            BookingCategoryType.Catering => "CATERING",
            BookingCategoryType.EquipmentRental => "EQUIPMENT_RENTAL",
            _ => "SPORTS"
        };
    }
}
