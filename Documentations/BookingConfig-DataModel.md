Bookazone Booking + Sports Config Data Model
===========================================

Purpose
-------
This document explains the booking and sports configuration tables, why they are related, and how data flows during tenant setup. It also summarizes the clean response DTOs we now return from the API.

High-level flow
---------------
1) BookingCategories are global (platform-wide).
2) Each tenant enables a subset of categories via TenantBookingCategories.
3) TenantWorkingHours define open hours + slot duration for the tenant.
4) TenantSportTypes define the sport offerings for the tenant.
5) SportResources are the actual bookable units (courts, pitches).
6) SportResourceAvailabilities define day-by-day local open hours for each resource.
7) SportResourceBlackouts block specific UTC time ranges.

Entity diagram (simplified)
---------------------------
Tenants
  |
  +--> TenantBookingCategories --> BookingCategories (global)
  |
  +--> TenantWorkingHours
  |
  +--> TenantSportTypes --> TenantBookingCategories
          |
          +--> SportResources
                  |
                  +--> SportResourceAvailabilities
                  |
                  +--> SportResourceBlackouts
                  |
                  +--> Bookings --> BookingApprovals

Tables and relationships
------------------------
BookingCategories (booking.BookingCategories)
- Why it exists: the global list of categories used by all tenants.
- Key fields: Name, Code, DefaultBookingMode.
- Constraints: Name and Code are unique.

TenantBookingCategories (booking.TenantBookingCategories)
- Why it exists: a tenant-specific enablement and override layer for categories.
- FK: FkTenantId -> tenants.Tenants.Id
- FK: FkBookingCategoryId -> booking.BookingCategories.Id
- Constraints: unique (FkTenantId, FkBookingCategoryId)
- Fields: IsEnabled, BookingModeOverride
- Effective booking mode = BookingModeOverride ?? BookingCategory.DefaultBookingMode

TenantWorkingHours (tenants.TenantWorkingHours)
- Why it exists: defines tenant-wide opening hours and slot duration.
- FK: FkTenantId -> tenants.Tenants.Id
- Constraints:
  - unique (FkTenantId, DayOfWeek)
  - if IsClosed = false, then OpenTime and CloseTime are required and OpenTime < CloseTime
- Fields: DayOfWeek, OpenTime, CloseTime, IsClosed, SlotDurationMinutes

TenantSportTypes (sports.TenantSportTypes)
- Why it exists: the tenant's list of sport offerings (Football, Tennis, etc.).
- FK: FkTenantId -> tenants.Tenants.Id
- FK: FkTenantBookingCategoryId -> booking.TenantBookingCategories.Id
- Constraints: unique (FkTenantId, Name)
- Fields: BookingMode, MinDurationMinutes, MaxDurationMinutes, BufferMinutes

SportResources (sports.SportResources)
- Why it exists: the actual bookable resources (Pitch A, Court 1, etc.).
- FK: FkTenantId -> tenants.Tenants.Id
- FK: FkTenantSportTypeId -> sports.TenantSportTypes.Id
- Index: (FkTenantId, FkTenantSportTypeId)
- Fields: Name, Description, Capacity, Address, Latitude, Longitude

SportResourceAvailabilities (sports.SportResourceAvailabilities)
- Why it exists: per-resource weekly opening hours (local time).
- FK: FkSportResourceId -> sports.SportResources.Id
- Index: (FkSportResourceId, DayOfWeek)
- Fields: DayOfWeek, OpenTime, CloseTime, IsClosed

SportResourceBlackouts (sports.SportResourceBlackouts)
- Why it exists: blocks specific time ranges for maintenance/events (UTC).
- FK: FkSportResourceId -> sports.SportResources.Id
- Index: (FkSportResourceId, StartUtc, EndUtc)
- Constraint: StartUtc < EndUtc
- Fields: StartUtc, EndUtc, Reason

SportResourcePricingRules (sports.SportResourcePricingRules)
- Why it exists: defines how a resource is priced (flat, per hour, per slot).
- FK: FkSportResourceId -> sports.SportResources.Id
- Index: (FkSportResourceId, DayOfWeek, StartTime, EndTime)
- Fields: RuleType, PriceAmount, Currency, DayOfWeek (optional), StartTime/EndTime (optional), MinDurationMinutes (optional)
- Selection: the most specific rule that matches the booking local day/time is applied.
- Validation: overlapping rules with the same day/time scope are rejected to avoid ambiguous pricing.

Bookings (booking.Bookings)
- Why it exists: customer reservations for a resource.
- FK: FkTenantId -> tenants.Tenants.Id
- FK: FkSportResourceId -> sports.SportResources.Id
- FK: FkCustomerId -> profile.Users.Id
- Fields: Status, BookingMode, StartUtc, EndUtc, ExpectedAttendees, TotalPrice, Currency, Notes
- Constraint: StartUtc < EndUtc
- Validation: uses tenant working hours, resource availability, blackouts, and overlap checks.
- Lifecycle: bookings are created as PendingApproval and require tenant approval before confirmation.

BookingApprovals (booking.BookingApprovals)
- Why it exists: approval flow for request-based bookings.
- FK: FkBookingId -> booking.Bookings.Id (one-to-one)
- Fields: Decision, DecisionAtUtc, Reason

Time semantics
--------------
- TenantWorkingHours and SportResourceAvailabilities use local time (TimeSpan).
- SportResourceBlackouts use UTC (DateTime).
- Booking slot enforcement comes from TenantWorkingHours.SlotDurationMinutes.

Clean response DTOs (API output)
--------------------------------
We return DTOs instead of raw entities to avoid leaking tenants, users, and audit fields.
- VwBookingCategory
- VwTenantBookingCategory
- VwTenantWorkingHour
- VwTenantSportType
- VwSportResource
- VwSportResourceAvailability
- VwSportResourceBlackout

Bulk endpoints
--------------
These endpoints accept a list payload and optionally support all-or-nothing transactions:
- Tenant category bulk upsert: /api/web/v1/secure/booking/config/tenant/category/bulk
- Tenant working hour bulk upsert: /api/web/v1/secure/booking/config/tenant/workinghour/bulk
- Resource availability bulk upsert: /api/web/v1/secure/booking/config/sport/resource/availability/bulk
- Resource pricing rule bulk upsert: /api/web/v1/secure/booking/config/sport/resource/pricing/bulk

Pass allOrNothing=true as a query parameter for atomic behavior.

Pricing rule endpoints
----------------------
Base: /api/web/v1/secure/booking/config/sport/resource/pricing
- GET /all?resourceId=...
- POST /create
- PUT /update
- DELETE /delete?id=...
- POST /bulk?allOrNothing=true
