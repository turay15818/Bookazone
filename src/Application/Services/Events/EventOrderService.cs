using Microsoft.EntityFrameworkCore;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Application.DTOs;
using Bookazone.Application.DTOs.Converter;
using Bookazone.Application.DTOs.Events;
using Bookazone.Application.DTOs.Response;
using Bookazone.Application.Interfaces.Context;
using Bookazone.Application.Interfaces.Repository.Events;
using Bookazone.Application.Interfaces.Services.Events;
using Bookazone.Domain.Entities.Events;
using Bookazone.Domain.Enums;

namespace Bookazone.Application.Services.Events;

public class EventOrderService(
    IEventOrderRepository eventOrderRepository,
    IEventOrderItemRepository eventOrderItemRepository,
    IEventRepository eventRepository,
    IEventTicketTypeRepository eventTicketTypeRepository,
    IUserContext userContext,
    ILogger<EventOrderService> logger)
    : IEventOrderService
{
    private static readonly TimeSpan ReservationWindow = TimeSpan.FromMinutes(15);

    public async Task<ApiResult> CreateOrderAsync(EventOrderCreateRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!userContext.UserId.HasValue)
                return ApiResponse.Error(ErrorHttp.Forbidden,
                    new Exception("User context does not contain a valid user id."));

            if (request.EventId == Guid.Empty)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "EventId is required.");

            if (request.Items == null || request.Items.Count == 0)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "At least one ticket item is required.");

            var evt = await eventRepository.Query()
                .AsNoTracking()
                .Include(e => e.FkVenue)
                .Include(e => e.FkCoverMedia)
                .FirstOrDefaultAsync(e =>
                    e.Id == request.EventId &&
                    e.Active &&
                    !e.Deleted,
                    cancellationToken);

            if (evt == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (evt.Status != EventStatus.Published)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Event is not published.");

            var nowUtc = DateTime.UtcNow;
            if (evt.EndUtc <= nowUtc)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Event has already ended.");

            await ExpireStaleOrdersAsync(evt.Id, null, null, cancellationToken);

            var groupedItems = request.Items
                .Where(i => i != null)
                .GroupBy(i => i.TicketTypeId)
                .Select(g => new { TicketTypeId = g.Key, Quantity = g.Sum(x => x.Quantity) })
                .ToList();

            if (groupedItems.Count == 0)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Ticket items are invalid.");

            if (groupedItems.Any(i => i.TicketTypeId == Guid.Empty || i.Quantity <= 0))
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Ticket quantities must be greater than zero.");

            var ticketTypeIds = groupedItems.Select(i => i.TicketTypeId).ToList();
            var quantityLookup = groupedItems.ToDictionary(i => i.TicketTypeId, i => i.Quantity);
            var ticketTypes = await eventTicketTypeRepository.Query()
                .AsNoTracking()
                .Where(t =>
                    ticketTypeIds.Contains(t.Id) &&
                    t.FkEventId == evt.Id &&
                    t.Active &&
                    !t.Deleted)
                .ToListAsync(cancellationToken);

            if (ticketTypes.Count != ticketTypeIds.Count)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "One or more ticket types are invalid.");

            var currency = NormalizeCurrency(ticketTypes[0].Currency);
            if (ticketTypes.Any(t => NormalizeCurrency(t.Currency) != currency))
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Ticket types must share the same currency.");

            var reservedLookup = await eventOrderItemRepository.GetReservedQuantitiesAsync(
                evt.Id,
                ticketTypeIds,
                nowUtc,
                cancellationToken);

            foreach (var ticket in ticketTypes)
            {
                var requestedQuantity = quantityLookup[ticket.Id];

                if (ticket.MinPerOrder.HasValue && requestedQuantity < ticket.MinPerOrder.Value)
                    return ApiResponse.ErrorWithMessage(
                        ErrorHttp.BadRequest,
                        $"Minimum quantity for {ticket.Name} is {ticket.MinPerOrder.Value}.");

                if (ticket.MaxPerOrder.HasValue && requestedQuantity > ticket.MaxPerOrder.Value)
                    return ApiResponse.ErrorWithMessage(
                        ErrorHttp.BadRequest,
                        $"Maximum quantity for {ticket.Name} is {ticket.MaxPerOrder.Value}.");

                if (ticket.SalesStartUtc.HasValue && nowUtc < ticket.SalesStartUtc.Value)
                    return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, $"{ticket.Name} sales have not started yet.");

                if (ticket.SalesEndUtc.HasValue && nowUtc > ticket.SalesEndUtc.Value)
                    return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, $"{ticket.Name} sales have ended.");

                if (ticket.Capacity.HasValue)
                {
                    reservedLookup.TryGetValue(ticket.Id, out var reserved);
                    var available = ticket.Capacity.Value - reserved;
                    if (available < requestedQuantity)
                        return ApiResponse.ErrorWithMessage(
                            ErrorHttp.BadRequest,
                            $"Only {Math.Max(available, 0)} tickets left for {ticket.Name}.");
                }
            }

            var totalAmount = 0m;
            var totalQuantity = 0;
            var orderItems = new List<EventOrderItem>();

            foreach (var ticket in ticketTypes)
            {
                var quantity = quantityLookup[ticket.Id];
                var subtotal = Math.Round(ticket.PriceAmount * quantity, 2, MidpointRounding.AwayFromZero);
                totalAmount += subtotal;
                totalQuantity += quantity;

                orderItems.Add(new EventOrderItem
                {
                    FkEventTicketTypeId = ticket.Id,
                    TicketName = ticket.Name,
                    UnitPrice = ticket.PriceAmount,
                    Currency = currency,
                    Quantity = quantity,
                    Subtotal = subtotal,
                    Active = true,
                    Deleted = false
                });
            }

            totalAmount = Math.Round(totalAmount, 2, MidpointRounding.AwayFromZero);

            var status = totalAmount <= 0m ? EventOrderStatus.Confirmed : EventOrderStatus.PendingPayment;
            var paymentStatus = totalAmount <= 0m ? EventPaymentStatus.NotRequired : EventPaymentStatus.Pending;
            DateTime? expiresAtUtc = totalAmount <= 0m ? null : nowUtc.Add(ReservationWindow);

            var order = new EventOrder
            {
                FkEventId = evt.Id,
                FkTenantId = evt.FkTenantId,
                FkCustomerId = userContext.UserId.Value,
                Status = status,
                PaymentStatus = paymentStatus,
                TotalAmount = totalAmount,
                Currency = currency,
                TotalQuantity = totalQuantity,
                ExpiresAtUtc = expiresAtUtc,
                Notes = NormalizeNotes(request.Notes),
                ContactName = NormalizeContact(request.Contact?.Name),
                ContactEmail = NormalizeContact(request.Contact?.Email),
                ContactPhone = NormalizeContact(request.Contact?.Phone),
                Active = true,
                Deleted = false
            };

            await using var transaction = await eventOrderRepository.BeginTransactionAsync();
            var created = await eventOrderRepository.AddAsync(order);

            foreach (var item in orderItems)
            {
                item.FkEventOrderId = created.Id;
                await eventOrderItemRepository.AddAsync(item);
            }

            await transaction.CommitAsync(cancellationToken);

            var detail = EventOrderConverter.ToDto(created, orderItems, evt, evt.FkVenue, evt.FkCoverMedia);
            return ApiResponse.Success(detail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating event order");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> ConfirmOrderAsync(EventOrderConfirmRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await GetOrderForUpdateAsync(request.OrderId, cancellationToken);
            if (order == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (!IsTenantActor(order.FkTenantId))
                return ApiResponse.Error(ErrorHttp.Forbidden, new Exception("User is not authorized for this tenant."));

            if (await ExpireOrderIfNeededAsync(order, cancellationToken))
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Order has expired.");

            if (order.Status is EventOrderStatus.Cancelled or EventOrderStatus.Expired or EventOrderStatus.Completed)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Order cannot be confirmed in its current status.");

            if (order.Status == EventOrderStatus.Confirmed)
                return await GetOrderAsync(order.Id, cancellationToken);

            order.Status = EventOrderStatus.Confirmed;
            await eventOrderRepository.UpdateAsync(order);

            return await GetOrderAsync(order.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error confirming event order {OrderId}", request.OrderId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> CancelOrderAsync(EventOrderCancelRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await GetOrderForUpdateAsync(request.OrderId, cancellationToken);
            if (order == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (!IsTenantActor(order.FkTenantId) && !IsCustomerActor(order))
                return ApiResponse.Error(ErrorHttp.Forbidden, new Exception("User is not authorized to cancel this order."));

            if (await ExpireOrderIfNeededAsync(order, cancellationToken))
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Order has expired.");

            if (order.Status is EventOrderStatus.Cancelled or EventOrderStatus.Expired or EventOrderStatus.Completed)
                return ApiResponse.ErrorWithMessage(ErrorHttp.BadRequest, "Order cannot be cancelled in its current status.");

            order.Status = EventOrderStatus.Cancelled;
            order.Notes = NormalizeNotes(request.Reason) ?? order.Notes;
            await eventOrderRepository.UpdateAsync(order);

            return await GetOrderAsync(order.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error cancelling event order {OrderId}", request.OrderId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> GetOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        try
        {
            var order = await GetOrderForUpdateAsync(orderId, cancellationToken);
            if (order == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            if (!IsTenantActor(order.FkTenantId) && !IsCustomerActor(order))
                return ApiResponse.Error(ErrorHttp.Forbidden, new Exception("User is not authorized to view this order."));

            if (await ExpireOrderIfNeededAsync(order, cancellationToken))
                order = await GetOrderForUpdateAsync(orderId, cancellationToken);

            if (order == null)
                return ApiResponse.Error(ErrorHttp.NotFound);

            var evt = await eventRepository.Query()
                .AsNoTracking()
                .Include(e => e.FkVenue)
                .Include(e => e.FkCoverMedia)
                .FirstOrDefaultAsync(e =>
                    e.Id == order.FkEventId &&
                    e.Active &&
                    !e.Deleted,
                    cancellationToken);

            var items = await eventOrderItemRepository.GetByOrderAsync(order.Id, cancellationToken);
            var detail = EventOrderConverter.ToDto(order, items, evt, evt?.FkVenue, evt?.FkCoverMedia);
            return ApiResponse.Success(detail);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting event order {OrderId}", orderId);
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> GetCustomerOrdersAsync(
        List<EventOrderStatus>? statuses = null,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        Guid? eventId = null,
        int? pageIndex = null,
        int? pageSize = null,
        string? sortBy = null,
        string? sortDirection = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!userContext.UserId.HasValue)
                return ApiResponse.Error(ErrorHttp.Forbidden,
                    new Exception("User context does not contain a valid user id."));

            var customerId = userContext.UserId.Value;
            await ExpireStaleOrdersAsync(null, null, customerId, cancellationToken);

            var query = eventOrderRepository.Query()
                .AsNoTracking()
                .Where(o =>
                    o.FkCustomerId == customerId &&
                    o.Active &&
                    !o.Deleted);

            if (eventId.HasValue)
                query = query.Where(o => o.FkEventId == eventId.Value);

            query = ApplyRangeFilter(query, fromUtc, toUtc);

            if (statuses is { Count: > 0 })
                query = query.Where(o => statuses.Contains(o.Status));

            var projected = query.Select(o => new VwEventOrderListItem
            {
                Id = o.Id,
                EventId = o.FkEventId,
                EventTitle = o.FkEvent.Title,
                StartUtc = o.FkEvent.StartUtc,
                EndUtc = o.FkEvent.EndUtc,
                Status = o.Status,
                PaymentStatus = o.PaymentStatus,
                TotalAmount = o.TotalAmount,
                Currency = o.Currency,
                TotalQuantity = o.TotalQuantity,
                CreatedAtUtc = o.DateCreated,
                TenantId = o.FkTenantId,
                TenantName = o.FkTenant.Name,
                CustomerId = o.FkCustomerId,
                CustomerName = ((o.FkCustomer.Firstname ?? "") + " " + (o.FkCustomer.Lastname ?? "")).Trim()
            });

            var sorted = ApplySorting(projected, sortBy, sortDirection);
            var paged = await Paginate<VwEventOrderListItem>.CreateAsync(sorted, pageIndex, pageSize);
            return ApiResponse.Success(sorted, "success", pageSize, pageIndex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting customer event orders");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    public async Task<ApiResult> GetTenantOrdersAsync(
        List<EventOrderStatus>? statuses = null,
        DateTime? fromUtc = null,
        DateTime? toUtc = null,
        Guid? eventId = null,
        Guid? customerId = null,
        int? pageIndex = null,
        int? pageSize = null,
        string? sortBy = null,
        string? sortDirection = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!userContext.TenantId.HasValue)
                return ApiResponse.Error(ErrorHttp.Forbidden,
                    new Exception("User context does not contain a valid tenant id."));

            var tenantId = userContext.TenantId.Value;
            await ExpireStaleOrdersAsync(null, tenantId, null, cancellationToken);

            var query = eventOrderRepository.Query()
                .AsNoTracking()
                .Where(o =>
                    o.FkTenantId == tenantId &&
                    o.Active &&
                    !o.Deleted);

            if (eventId.HasValue)
                query = query.Where(o => o.FkEventId == eventId.Value);

            if (customerId.HasValue)
                query = query.Where(o => o.FkCustomerId == customerId.Value);

            query = ApplyRangeFilter(query, fromUtc, toUtc);

            if (statuses is { Count: > 0 })
                query = query.Where(o => statuses.Contains(o.Status));

            var projected = query.Select(o => new VwEventOrderListItem
            {
                Id = o.Id,
                EventId = o.FkEventId,
                EventTitle = o.FkEvent.Title,
                StartUtc = o.FkEvent.StartUtc,
                EndUtc = o.FkEvent.EndUtc,
                Status = o.Status,
                PaymentStatus = o.PaymentStatus,
                TotalAmount = o.TotalAmount,
                Currency = o.Currency,
                TotalQuantity = o.TotalQuantity,
                CreatedAtUtc = o.DateCreated,
                TenantId = o.FkTenantId,
                TenantName = o.FkTenant.Name,
                CustomerId = o.FkCustomerId,
                CustomerName = ((o.FkCustomer.Firstname ?? "") + " " + (o.FkCustomer.Lastname ?? "")).Trim()
            });

            var sorted = ApplySorting(projected, sortBy, sortDirection);
            var paged = await Paginate<VwEventOrderListItem>.CreateAsync(sorted, pageIndex, pageSize);
            return ApiResponse.Success(paged);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting tenant event orders");
            return ApiResponse.Error(ErrorHttp.Error, ex);
        }
    }

    private async Task<bool> ExpireOrderIfNeededAsync(EventOrder order, CancellationToken cancellationToken)
    {
        if (order.Status != EventOrderStatus.PendingPayment)
            return false;

        if (!order.ExpiresAtUtc.HasValue || order.ExpiresAtUtc.Value > DateTime.UtcNow)
            return false;

        order.Status = EventOrderStatus.Expired;
        await eventOrderRepository.UpdateAsync(order);
        return true;
    }

    private async Task ExpireStaleOrdersAsync(
        Guid? eventId,
        Guid? tenantId,
        Guid? customerId,
        CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;
        var query = eventOrderRepository.Query()
            .IgnoreAutoIncludes()
            .Where(o =>
                o.Status == EventOrderStatus.PendingPayment &&
                o.ExpiresAtUtc.HasValue &&
                o.ExpiresAtUtc.Value <= nowUtc &&
                o.Active &&
                !o.Deleted);

        if (eventId.HasValue)
            query = query.Where(o => o.FkEventId == eventId.Value);

        if (tenantId.HasValue)
            query = query.Where(o => o.FkTenantId == tenantId.Value);

        if (customerId.HasValue)
            query = query.Where(o => o.FkCustomerId == customerId.Value);

        var staleOrders = await query.ToListAsync(cancellationToken);
        foreach (var order in staleOrders)
        {
            order.Status = EventOrderStatus.Expired;
            await eventOrderRepository.UpdateAsync(order);
        }
    }

    private static IQueryable<EventOrder> ApplyRangeFilter(
        IQueryable<EventOrder> query,
        DateTime? fromUtc,
        DateTime? toUtc)
    {
        if (fromUtc.HasValue)
            query = query.Where(o => o.FkEvent.EndUtc >= fromUtc.Value);

        if (toUtc.HasValue)
            query = query.Where(o => o.FkEvent.StartUtc <= toUtc.Value);

        return query;
    }

    private static IQueryable<VwEventOrderListItem> ApplySorting(
        IQueryable<VwEventOrderListItem> query,
        string? sortBy,
        string? sortDirection)
    {
        var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
        var key = sortBy?.Trim().ToLowerInvariant();

        return key switch
        {
            "startutc" or "start" => descending ? query.OrderByDescending(x => x.StartUtc) : query.OrderBy(x => x.StartUtc),
            "createdutc" or "created" => descending ? query.OrderByDescending(x => x.CreatedAtUtc) : query.OrderBy(x => x.CreatedAtUtc),
            "status" => descending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            "total" or "totalamount" => descending ? query.OrderByDescending(x => x.TotalAmount) : query.OrderBy(x => x.TotalAmount),
            _ => query.OrderByDescending(x => x.StartUtc)
        };
    }

    private Task<EventOrder?> GetOrderForUpdateAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return eventOrderRepository.Query()
            .IgnoreAutoIncludes()
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.Deleted, cancellationToken);
    }

    private bool IsTenantActor(Guid tenantId)
        => userContext.TenantId.HasValue && userContext.TenantId.Value == tenantId;

    private bool IsCustomerActor(EventOrder order)
        => userContext.UserId.HasValue && order.FkCustomerId == userContext.UserId.Value;

    private static string NormalizeCurrency(string? currency)
    {
        return string.IsNullOrWhiteSpace(currency)
            ? "USD"
            : currency.Trim().ToUpperInvariant();
    }

    private static string? NormalizeNotes(string? notes)
        => string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();

    private static string? NormalizeContact(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}