Bookazone Event Ticketing API (Phase 1)
=======================================

Purpose
-------
This document formalizes the ticket-booking flow for events before payment integration. Customers can reserve tickets, receive an order in PendingPayment, and later confirm when payments are added. Free events can auto-confirm.

Swagger grouping
----------------
Swagger doc group: API Events v1

Secure endpoints (auth required)
--------------------------------
Base: /api/web/v1/secure/events/order

1) Create ticket order (reserve capacity)
POST /create
Body: EventOrderCreateRequest
Response: VwEventOrder

EventOrderCreateRequest
{
  "eventId": "11111111-1111-1111-1111-111111111111",
  "items": [
    { "ticketTypeId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa", "quantity": 2 },
    { "ticketTypeId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb", "quantity": 1 }
  ],
  "contact": {
    "name": "Jane Doe",
    "email": "jane@example.com",
    "phone": "+232-76-555-111"
  },
  "notes": "Seats near the front if possible."
}

Success response (example)
{
  "status": 1,
  "message": "success",
  "data": {
    "id": "22222222-2222-2222-2222-222222222222",
    "eventId": "11111111-1111-1111-1111-111111111111",
    "tenantId": "33333333-3333-3333-3333-333333333333",
    "customerId": "44444444-4444-4444-4444-444444444444",
    "status": "PendingPayment",
    "paymentStatus": "Pending",
    "totalAmount": 450,
    "currency": "SLE",
    "totalQuantity": 3,
    "expiresAtUtc": "2026-02-14T20:15:00Z",
    "createdAtUtc": "2026-02-14T20:00:00Z",
    "items": [
      {
        "ticketTypeId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
        "ticketName": "Regular",
        "quantity": 2,
        "unitPrice": 150,
        "currency": "SLE",
        "subtotal": 300
      },
      {
        "ticketTypeId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb",
        "ticketName": "VIP",
        "quantity": 1,
        "unitPrice": 150,
        "currency": "SLE",
        "subtotal": 150
      }
    ],
    "event": {
      "title": "Afro Night Live Concert",
      "startUtc": "2026-02-14T20:00:00Z",
      "endUtc": "2026-02-14T23:30:00Z",
      "city": "Freetown",
      "venueName": "Lumley Beach Stage",
      "coverUrl": "https://images.example.com/cover.jpg"
    }
  }
}

Notes:
- The server calculates prices from EventTicketTypes. Client prices are ignored.
- If totalAmount == 0, the order is auto-confirmed and paymentStatus = NotRequired.
- PendingPayment orders should expire after a short window (e.g., 10-15 minutes).

2) Confirm ticket order (tenant/admin or free events)
POST /confirm
Body: EventOrderConfirmRequest
Response: VwEventOrder

EventOrderConfirmRequest
{
  "orderId": "22222222-2222-2222-2222-222222222222"
}

3) Cancel ticket order (customer or tenant)
POST /cancel
Body: EventOrderCancelRequest
Response: VwEventOrder

EventOrderCancelRequest
{
  "orderId": "22222222-2222-2222-2222-222222222222",
  "reason": "Change of plans"
}

4) Get order details
GET /find?id=...
Response: VwEventOrder

5) List customer orders
GET /customer
Query parameters:
- statuses (optional, repeatable): PendingPayment, Confirmed, Cancelled, Expired, Completed
- fromUtc (optional)
- toUtc (optional)
- eventId (optional)
- pageIndex (optional, default 1)
- pageSize (optional, default 100)
- sortBy (optional): createdUtc, startUtc, status, total
- sortDirection (optional): asc, desc
Response: DataPaginate<VwEventOrderListItem>

6) List tenant orders (tenant admin)
GET /tenant
Query parameters:
- statuses (optional, repeatable)
- fromUtc (optional)
- toUtc (optional)
- eventId (optional)
- customerId (optional)
- pageIndex (optional)
- pageSize (optional)
- sortBy (optional)
- sortDirection (optional)
Response: DataPaginate<VwEventOrderListItem>

DTO shapes (summary)
--------------------
EventOrderCreateRequest
- eventId (required)
- items[]: ticketTypeId, quantity (required)
- contact: name, email, phone (optional)
- notes (optional)

VwEventOrder
- id, eventId, tenantId, customerId
- status, paymentStatus
- totalAmount, currency, totalQuantity
- expiresAtUtc, createdAtUtc
- items[]: ticketTypeId, ticketName, quantity, unitPrice, currency, subtotal
- event summary (optional)

VwEventOrderListItem
- id, eventId, eventTitle, startUtc, status, paymentStatus, totalAmount, currency, totalQuantity

Enum values (recommended)
-------------------------
EventOrderStatus:
- PendingPayment
- Confirmed
- Cancelled
- Expired
- Completed

EventPaymentStatus:
- NotRequired
- Pending
- Paid
- Failed
- Refunded

Availability behavior
---------------------
- Capacity is reserved at order creation and released on cancel/expire.
- Remaining capacity can be calculated as:
  capacity - sum(orderItem.quantity) for orders in PendingPayment or Confirmed.

