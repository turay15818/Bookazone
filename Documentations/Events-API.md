Bookazone Events API
====================

Purpose
-------
This document describes the public event discovery APIs and the secure event management APIs. These endpoints are grouped under the Swagger doc "API Events v1".

Swagger grouping
----------------
Swagger doc group: API Events v1

Public endpoints (no auth)
--------------------------
Base: /api/web/v1/public/events

1) List event categories (chips)
GET /categories
Response: list of VwEventCategory

2) List events (cards)
GET /all
Query parameters:
- categoryId (optional)
- categoryCode (optional)
- tenantId (optional)
- city (optional)
- search (optional, matches title/subtitle/tenant)
- fromUtc (optional)
- toUtc (optional)
- pageIndex (optional, default 1)
- pageSize (optional, default 100)
- sortBy (optional): startUtc, endUtc, title, price, city
- sortDirection (optional): asc, desc
Response: DataPaginate<VwEventCard>

3) Get event details
GET /{id}
Response: VwEventDetail

4) Similar events
GET /{id}/similar
Query parameters:
- take (optional, default 6)
Response: list of VwEventCard

Secure endpoints (auth required)
--------------------------------
Base: /api/web/v1/secure/events

1) List tenant events
GET /all
Query parameters:
- statuses (optional, repeatable): Draft, Published, Cancelled, Completed
- fromUtc (optional)
- toUtc (optional)
- categoryId (optional)
- search (optional)
- pageIndex (optional)
- pageSize (optional)
- sortBy (optional): startUtc, endUtc, title, status, price
- sortDirection (optional): asc, desc
Response: DataPaginate<VwEventListItem>

2) Get event by id
GET /find?id=...
Response: VwEventDetail

3) Create event
POST /create
Body: EventCreateRequest
- tenantId
- eventCategoryId
- title
- subtitle (optional)
- about (optional)
- startUtc
- endUtc
- timeZoneId (optional)
- city (optional)
- priceFrom (optional)
- venue (optional)
- ticketTypes (optional list)
- schedule (optional list)
- policies (optional list)
- media (optional list)

4) Update event
PUT /update
Body: EventUpdateRequest

5) Publish event
POST /publish
Body: EventPublishRequest

6) Cancel event
POST /cancel
Body: EventCancelRequest

7) Delete event
DELETE /delete?id=...

8) Upsert event venue
POST /{eventId}/venue
Body: EventVenueUpsertRequest

9) Bulk upsert ticket types
POST /{eventId}/tickets/bulk?allOrNothing=true
Body: List<EventTicketTypeUpsertRequest>

10) Bulk upsert schedule items
POST /{eventId}/schedule/bulk?allOrNothing=true
Body: List<EventScheduleItemUpsertRequest>

11) Bulk upsert policies
POST /{eventId}/policies/bulk?allOrNothing=true
Body: List<EventPolicyUpsertRequest>

12) Bulk upsert media
POST /{eventId}/media/bulk?allOrNothing=true
Body: List<EventMediaUpsertRequest>

Event subscriptions (secure)
----------------------------
Base: /api/web/v1/secure/events/subscription

1) List my subscriptions
GET /my
Response: list of VwEventCategorySubscription

2) Upsert subscription
POST /upsert
Body: EventCategorySubscriptionUpsertRequest
- eventCategoryId
- subscribe (true/false)
- receivePush (true/false)
- receiveEmail (true/false)

3) Bulk upsert subscriptions
POST /bulk?allOrNothing=true
Body: List<EventCategorySubscriptionUpsertRequest>

Event category config (secure)
------------------------------
Base: /api/web/v1/secure/events/category

1) List categories
GET /all

2) Find category
GET /find?id=...

3) Create category
POST /create
Body: EventCategoryCreateRequest

4) Update category
PUT /update
Body: EventCategoryUpdateRequest

5) Delete category
DELETE /delete?id=...

Notes
-----
- Public endpoints return only Published events.
- Tenants must enable BookingCategory code EVENTS before publishing events.
- Ticket perks are stored as a JSON list in EventTicketTypes.Perks.
- Event detail ticket types include AvailableQuantity (null means unlimited capacity).
- Publishing an event triggers push + email notifications to subscribers of its category.
