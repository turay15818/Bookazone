using Bookazone.Application.DTOs.Response;
using Bookazone.Domain.Entities.Vehicles;
using Bookazone.Domain.Enums;

namespace Bookazone.Application.DTOs.Converter;

public static class VehicleOrderConverter
{
    public static VwVehicleOrderListItem ToListItem(
        VehicleOrder order,
        string? coverUrl,
        string? customerName,
        string? customerEmail,
        string? tenantName,
        string? vehicleTitle)
    {
        return new VwVehicleOrderListItem
        {
            Id = order.Id,
            VehicleId = order.FkVehicleId,
            VehicleTitle = vehicleTitle ?? order.FkVehicle?.Title ?? string.Empty,
            VehicleType = order.FkVehicle?.Type ?? VehicleType.Car,
            ServiceType = order.ServiceType,
            VehicleCoverUrl = coverUrl,
            TenantId = order.FkTenantId,
            TenantName = tenantName ?? order.FkTenant?.Name ?? string.Empty,
            CustomerId = order.FkCustomerId,
            CustomerName = customerName ?? string.Empty,
            CustomerEmail = customerEmail,
            Status = order.Status,
            PricingUnit = order.PricingUnit,
            Quantity = order.Quantity,
            UnitPrice = order.UnitPrice,
            TotalPrice = order.TotalPrice,
            Currency = order.Currency,
            StartUtc = order.StartUtc,
            EndUtc = order.EndUtc,
            DateCreated = order.DateCreated
        };
    }

    public static VwVehicleOrderDetail ToDetail(
        VehicleOrder order,
        string? coverUrl,
        string? customerName,
        string? customerEmail,
        string? tenantName,
        string? vehicleTitle)
    {
        var listItem = ToListItem(order, coverUrl, customerName, customerEmail, tenantName, vehicleTitle);
        return new VwVehicleOrderDetail
        {
            Id = listItem.Id,
            VehicleId = listItem.VehicleId,
            VehicleTitle = listItem.VehicleTitle,
            VehicleType = listItem.VehicleType,
            ServiceType = listItem.ServiceType,
            VehicleCoverUrl = listItem.VehicleCoverUrl,
            TenantId = listItem.TenantId,
            TenantName = listItem.TenantName,
            CustomerId = listItem.CustomerId,
            CustomerName = listItem.CustomerName,
            CustomerEmail = listItem.CustomerEmail,
            Status = listItem.Status,
            PricingUnit = listItem.PricingUnit,
            Quantity = listItem.Quantity,
            UnitPrice = listItem.UnitPrice,
            TotalPrice = listItem.TotalPrice,
            Currency = listItem.Currency,
            StartUtc = listItem.StartUtc,
            EndUtc = listItem.EndUtc,
            DateCreated = listItem.DateCreated,
            PickupAddress = order.PickupAddress,
            DropoffAddress = order.DropoffAddress,
            PickupLatitude = order.PickupLatitude,
            PickupLongitude = order.PickupLongitude,
            DropoffLatitude = order.DropoffLatitude,
            DropoffLongitude = order.DropoffLongitude,
            CargoDescription = order.CargoDescription,
            CargoWeightTons = order.CargoWeightTons,
            CargoVolumeCubicMeters = order.CargoVolumeCubicMeters,
            DriverRequested = order.DriverRequested,
            ContactName = order.ContactName,
            ContactEmail = order.ContactEmail,
            ContactPhone = order.ContactPhone,
            Notes = order.Notes
        };
    }
}
