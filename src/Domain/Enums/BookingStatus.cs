namespace Bookazone.Domain.Enums;

public enum BookingStatus
{
    PendingApproval = 0, // Waiting for tenant approval
    Confirmed = 1,       // Approved and scheduled
    Declined = 2,        // Rejected by tenant
    Cancelled = 3,       // Cancelled by customer or tenant
    Completed = 4        // Finished and closed
}
