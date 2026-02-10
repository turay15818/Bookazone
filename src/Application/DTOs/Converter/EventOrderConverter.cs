using Bookazone.Application.DTOs.Response;
using Bookazone.Domain.Entities.Events;

namespace Bookazone.Application.DTOs.Converter;

public static class EventOrderConverter
{
    public static VwEventOrderItem ToDto(EventOrderItem item)
    {
        return new VwEventOrderItem
        {
            TicketTypeId = item.FkEventTicketTypeId,
            TicketName = item.TicketName,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            Currency = item.Currency,
            Subtotal = item.Subtotal
        };
    }

    public static VwEventOrder ToDto(
        EventOrder order,
        IEnumerable<EventOrderItem> items,
        Event? evt,
        EventVenue? venue,
        EventMedia? cover)
    {
        return new VwEventOrder
        {
            Id = order.Id,
            EventId = order.FkEventId,
            TenantId = order.FkTenantId,
            CustomerId = order.FkCustomerId,
            Status = order.Status,
            PaymentStatus = order.PaymentStatus,
            TotalAmount = order.TotalAmount,
            Currency = order.Currency,
            TotalQuantity = order.TotalQuantity,
            ExpiresAtUtc = order.ExpiresAtUtc,
            CreatedAtUtc = order.DateCreated,
            Notes = order.Notes,
            ContactName = order.ContactName,
            ContactEmail = order.ContactEmail,
            ContactPhone = order.ContactPhone,
            Items = items.Select(ToDto).ToList(),
            Event = evt == null
                ? null
                : new VwEventOrderEvent
                {
                    Title = evt.Title,
                    StartUtc = evt.StartUtc,
                    EndUtc = evt.EndUtc,
                    City = evt.City,
                    VenueName = venue?.Name,
                    CoverUrl = cover?.Url
                }
        };
    }
}
