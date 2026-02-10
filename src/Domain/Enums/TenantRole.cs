namespace Bookazone.Domain.Enums;

public enum TenantRole
{
    Customer = 0,      // A normal user who books resources
    Staff = 1,         // Employee who manages bookings or resources
    Admin = 2          // Tenant/company admin with full control
}
