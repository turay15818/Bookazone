Bookazone Events Data Model
===========================

Purpose
-------
This document explains the events data model, how tables relate, and how data flows when a tenant publishes events.

High-level flow
---------------
1) EventCategories are global (platform-wide). They power the public category chips.
2) Tenants must enable the BookingCategory code EVENTS to create/publish events.
3) Events are created by tenants and can be Draft, Published, Cancelled, or Completed.
4) EventVenues store address and map coordinates.
5) EventTicketTypes define ticket pricing and limits.
6) EventScheduleItems define the agenda (start time + title).
7) EventPolicies store refund, entry, and security rules.
8) EventMedia holds images and cover art.

Entity diagram (simplified)
---------------------------
Tenants
  |
  +--> TenantBookingCategories --> BookingCategories (global; includes EVENTS)
  |
  +--> Events --> EventCategories (global)
          |
          +--> EventVenues (optional)
          +--> EventTicketTypes
          +--> EventScheduleItems
          +--> EventPolicies
          +--> EventMedia (cover + gallery)
          +--> EventOrders --> EventOrderItems --> EventTicketTypes

Users
  |
  +--> EventCategorySubscriptions --> EventCategories (global)

Tables and relationships
------------------------
EventCategories (events.EventCategories)
- Why it exists: the global list of event genres (Music, Comedy, Tech, etc.).
- Key fields: Name, Code, Description.
- Constraints: Name and Code are unique.

EventCategorySubscriptions (events.EventCategorySubscriptions)
- Why it exists: stores customer notification subscriptions by event category.
- FK: FkEventCategoryId -> events.EventCategories.Id
- FK: FkUserId -> profile.Users.Id
- Fields: ReceivePush, ReceiveEmail, Active, Deleted.
- Constraints: unique (FkUserId, FkEventCategoryId).

Events (events.Events)
- Why it exists: the core event listing for a tenant.
- FK: FkTenantId -> tenants.Tenants.Id
- FK: FkEventCategoryId -> events.EventCategories.Id
- FK: FkVenueId -> events.EventVenues.Id (optional)
- FK: FkCoverMediaId -> events.EventMedia.Id (optional)
- Fields: Title, Subtitle, About, StartUtc, EndUtc, TimeZoneId, Status, City, PriceFrom.
- Constraint: StartUtc < EndUtc.

EventVenues (events.EventVenues)
- Why it exists: venue metadata (address + coordinates) separate from the event.
- Fields: Name, Address, City, State, Country, Latitude, Longitude.

EventTicketTypes (events.EventTicketTypes)
- Why it exists: ticket pricing and limits per event.
- FK: FkEventId -> events.Events.Id
- Constraints: unique (FkEventId, Name).
- Fields: Name, PriceAmount, Currency, Capacity, MinPerOrder, MaxPerOrder, SalesStartUtc, SalesEndUtc, Perks.
- Notes: Perks is stored as a JSON list in the Perks column.

EventScheduleItems (events.EventScheduleItems)
- Why it exists: agenda timeline items for the event.
- FK: FkEventId -> events.Events.Id
- Fields: StartUtc, Title, Subtitle, SortOrder.

EventPolicies (events.EventPolicies)
- Why it exists: refund and entry policies for the event.
- FK: FkEventId -> events.Events.Id
- Fields: Title, Body, SortOrder.

EventMedia (events.EventMedia)
- Why it exists: gallery and cover images for the event.
- FK: FkEventId -> events.Events.Id
- Fields: Url, Caption, IsCover, SortOrder.
- Notes: the first IsCover media becomes Events.FkCoverMediaId.

Ticketing tables (phase 1)
--------------------------
EventOrders (events.EventOrders)
- Why it exists: a customer order that reserves ticket capacity before payment.
- FK: FkEventId -> events.Events.Id
- FK: FkTenantId -> tenants.Tenants.Id (denormalized for filtering)
- FK: FkCustomerId -> profile.Users.Id
- Fields: Status, PaymentStatus, TotalAmount, Currency, TotalQuantity, ExpiresAtUtc, Notes,
  ContactName, ContactEmail, ContactPhone.
- Notes: PendingPayment orders should expire and release capacity.

EventOrderItems (events.EventOrderItems)
- Why it exists: line items for each ticket type in an order.
- FK: FkEventOrderId -> events.EventOrders.Id
- FK: FkEventTicketTypeId -> events.EventTicketTypes.Id
- Fields: TicketName, UnitPrice, Currency, Quantity, Subtotal.
- Notes: TicketName is a snapshot to preserve history if ticket names change.

Time semantics
--------------
- Event StartUtc/EndUtc and schedule item StartUtc are stored as UTC.
- TimeZoneId is stored for local display in the client.

Status lifecycle
----------------
- Draft: created, not visible publicly.
- Published: visible in public discovery.
- Cancelled: not bookable/visible for new customers.
- Completed: historical.

Gatekeeping
-----------
- Tenants must enable BookingCategories code EVENTS in booking.TenantBookingCategories to create/publish events.
