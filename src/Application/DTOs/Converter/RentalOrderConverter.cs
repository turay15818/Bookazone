using Bookazone.Application.DTOs.Response;
using Bookazone.Domain.Entities.Rentals;
using Bookazone.Domain.Enums;

namespace Bookazone.Application.DTOs.Converter;

public static class RentalOrderConverter
{
    public static VwRentalOrderListItem ToListItem(
        RentalOrder order,
        string? coverUrl,
        string? customerName,
        string? customerEmail,
        string? tenantName,
        string? rentalTitle,
        RentalType rentalType)
    {
        return new VwRentalOrderListItem
        {
            Id = order.Id,
            RentalId = order.FkRentalId,
            RentalTitle = rentalTitle ?? order.FkRental?.Title ?? string.Empty,
            RentalType = rentalType,
            RentalCoverUrl = coverUrl,
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

    public static VwRentalOrderDetail ToDetail(
        RentalOrder order,
        string? coverUrl,
        string? customerName,
        string? customerEmail,
        string? tenantName,
        string? rentalTitle,
        RentalType rentalType)
    {
        var listItem = ToListItem(order, coverUrl, customerName, customerEmail, tenantName, rentalTitle, rentalType);
        return new VwRentalOrderDetail
        {
            Id = listItem.Id,
            RentalId = listItem.RentalId,
            RentalTitle = listItem.RentalTitle,
            RentalType = listItem.RentalType,
            RentalCoverUrl = listItem.RentalCoverUrl,
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
            GuestCount = order.GuestCount,
            ContactName = order.ContactName,
            ContactEmail = order.ContactEmail,
            ContactPhone = order.ContactPhone,
            Notes = order.Notes
        };
    }
}
