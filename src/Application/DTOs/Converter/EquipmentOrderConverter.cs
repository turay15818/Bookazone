using Bookazone.Application.DTOs.Response;
using Bookazone.Domain.Entities.Equipment;

namespace Bookazone.Application.DTOs.Converter;

public static class EquipmentOrderConverter
{
    public static VwEquipmentOrderListItem ToListItem(
        EquipmentOrder order,
        string? coverUrl,
        string? customerName,
        string? customerEmail,
        string? tenantName,
        string? equipmentTitle,
        string? equipmentCategory)
    {
        return new VwEquipmentOrderListItem
        {
            Id = order.Id,
            EquipmentId = order.FkEquipmentId,
            EquipmentTitle = equipmentTitle ?? order.FkEquipment?.Title ?? string.Empty,
            EquipmentCategory = equipmentCategory ?? order.FkEquipment?.Category,
            EquipmentCoverUrl = coverUrl,
            TenantId = order.FkTenantId,
            TenantName = tenantName ?? order.FkTenant?.Name ?? string.Empty,
            CustomerId = order.FkCustomerId,
            CustomerName = customerName ?? string.Empty,
            CustomerEmail = customerEmail,
            Status = order.Status,
            PricingUnit = order.PricingUnit,
            Quantity = order.Quantity,
            UnitsRequested = order.UnitsRequested,
            UnitPrice = order.UnitPrice,
            TotalPrice = order.TotalPrice,
            Currency = order.Currency,
            StartUtc = order.StartUtc,
            EndUtc = order.EndUtc,
            DateCreated = order.DateCreated
        };
    }

    public static VwEquipmentOrderDetail ToDetail(
        EquipmentOrder order,
        string? coverUrl,
        string? customerName,
        string? customerEmail,
        string? tenantName,
        string? equipmentTitle,
        string? equipmentCategory)
    {
        var listItem = ToListItem(order, coverUrl, customerName, customerEmail, tenantName, equipmentTitle, equipmentCategory);
        return new VwEquipmentOrderDetail
        {
            Id = listItem.Id,
            EquipmentId = listItem.EquipmentId,
            EquipmentTitle = listItem.EquipmentTitle,
            EquipmentCategory = listItem.EquipmentCategory,
            EquipmentCoverUrl = listItem.EquipmentCoverUrl,
            TenantId = listItem.TenantId,
            TenantName = listItem.TenantName,
            CustomerId = listItem.CustomerId,
            CustomerName = listItem.CustomerName,
            CustomerEmail = listItem.CustomerEmail,
            Status = listItem.Status,
            PricingUnit = listItem.PricingUnit,
            Quantity = listItem.Quantity,
            UnitsRequested = listItem.UnitsRequested,
            UnitPrice = listItem.UnitPrice,
            TotalPrice = listItem.TotalPrice,
            Currency = listItem.Currency,
            StartUtc = listItem.StartUtc,
            EndUtc = listItem.EndUtc,
            DateCreated = listItem.DateCreated,
            DeliveryAddress = order.DeliveryAddress,
            DeliveryLatitude = order.DeliveryLatitude,
            DeliveryLongitude = order.DeliveryLongitude,
            ContactName = order.ContactName,
            ContactEmail = order.ContactEmail,
            ContactPhone = order.ContactPhone,
            Notes = order.Notes
        };
    }
}
