namespace Bookazone.Domain.Enums;

public enum BookingApprovalDecision
{
    Pending = 0,   // Awaiting decision
    Approved = 1,  // Approved by tenant
    Declined = 2,  // Declined by tenant
    Cancelled = 3  // Cancelled after approval flow started
}
