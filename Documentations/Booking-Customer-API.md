Bookazone Customer Booking API (Sports)
=======================================

Purpose
-------
This document describes the customer-facing sports booking APIs. These endpoints are grouped under the Swagger doc "API Booking v1" to keep them separate from admin configuration APIs.

Swagger grouping
----------------
Swagger doc group: API Booking v1

Public endpoints (no auth)
--------------------------
Base: /api/web/v1/public/booking/sports

1) List sport types (filter chips)
GET /types
Query parameters:
- tenantId (optional)

Response: list of VwSportTypePublic

2) List sport resources (discover)
GET /resources
Query parameters:
- tenantId (optional)
- sportTypeId (optional)
- minCapacity (optional)
- search (optional, matches resource or tenant name)
- city (optional)

Response: list of VwSportResourcePublic (includes AvailabilityHours)

3) Get sport resource by id
GET /resources/{id}
Response: VwSportResourcePublic (includes AvailabilityHours)

4) Get resource availability (weekly schedule)
GET /resources/{id}/availability
Response: list of VwSportResourceAvailability

5) Get resource slots (available/unavailable for a date)
GET /resources/{id}/slots
Query parameters:
- dateLocal (required, yyyy-MM-dd in tenant local date)

Response: VwSportResourceSlotSummary (slots include StartLocal/EndLocal and StartUtc/EndUtc)

Public endpoints (tenants)
--------------------------
Base: /api/web/v1/public/booking/tenants

1) List tenants (optionally filtered by category)
GET /all
Query parameters:
- category (optional): Sports, Events, Vehicles, Spaces, Salons, Catering, EquipmentRental

Response: list of VwTenantPublic

Category enum mapping
---------------------
- Sports -> SPORTS
- Events -> EVENTS
- Vehicles -> VEHICLES
- Spaces -> SPACES
- Salons -> SALONS
- Catering -> CATERING
- EquipmentRental -> EQUIPMENT_RENTAL

Secure endpoints (auth required)
-------------------------------
Base: /api/web/v1/secure/booking

1) Validate a booking
POST /validate
Body:
- tenantId
- resourceId
- startUtc
- endUtc
- expectedAttendees (optional)

Returns BookingValidationResult

2) Create a booking
POST /create
Body:
- tenantId
- resourceId
- startUtc
- endUtc
- expectedAttendees (optional)
- notes
- bookingMode (optional, server forces approval flow)

Returns VwBooking

3) Approve a booking (tenant admin)
POST /approve
Body:
- bookingId
- reason (optional)

4) Decline a booking (tenant admin)
POST /decline
Body:
- bookingId
- reason (optional)

5) Cancel a booking (tenant admin or customer)
POST /cancel
Body:
- bookingId
- reason (optional)

6) Reschedule a booking (tenant admin or customer)
POST /reschedule
Body:
- bookingId
- startUtc
- endUtc
- expectedAttendees (optional)
- notes (optional)

7) List tenant bookings (tenant admin)
GET /tenant
Query parameters:
- statuses (optional, repeatable): PendingApproval, Confirmed, Declined, Cancelled, Completed
- fromUtc (optional)
- toUtc (optional)
- resourceId (optional)
- customerId (optional)
- pageIndex (optional, default 1)
- pageSize (optional, default 100)
- sortBy (optional): startUtc, endUtc, status, price, tenantName, resourceName, customerName
- sortDirection (optional): asc, desc

Returns DataPaginate<VwBookingListItem>

8) List customer bookings (customer)
GET /customer
Query parameters:
- statuses (optional, repeatable)
- fromUtc (optional)
- toUtc (optional)
- tenantId (optional)
- pageIndex (optional, default 1)
- pageSize (optional, default 100)
- sortBy (optional): startUtc, endUtc, status, price, tenantName, resourceName, customerName
- sortDirection (optional): asc, desc

Returns DataPaginate<VwBookingListItem>

Notes
-----
- Availability uses local time; blackouts are UTC. Validation converts UTC into tenant local time using TenantSettings.DefaultTimeZone (fallback UTC).
- If expectedAttendees is provided, it is validated against the resource capacity.
- Bookings are always created with Status=PendingApproval. Tenant admins must approve or decline.
- TotalPrice and Currency are calculated server-side from pricing rules (SportResourcePricingRules). Clients do not need to send price.
