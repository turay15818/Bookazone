namespace Bookazone.Application.Common.Shared.Utils
{
    /// <summary>
    /// Bookazone Pro+++ constants: roles, permissions, groups, and defaults.
    /// </summary>
    public static class Consts
    {
        #region Company Settings

        public static class CompanySetting
        {
            public const string NameShort = "Bookazone";
            public const string Name = "Bookazone";
            public const string NameLong = "Bookazone Sierra Leone";
            public const string GlobalBrandName = "BAZ";
        }
        
        #endregion
        
        /// <summary>
        /// General application configuration constants
        /// </summary>
        
        public static class AppSetting { public const string FrontendUrl = "http://localhost:3000"; }
        public static class AppConfig { public const string AppStatusKey = "APP_STATUS"; 
            public const string AppStatusKeyV2 = "APP_STATUS_V1"; 
            public const string SecondDeviceCountdown = "SECOND_DEVICE_COUNTDOWN"; }
        public static class AppStatus { public const string Active = "ACTIVE"; public const string Inactive = "INACTIVE"; }

        #region Security & Authorization

        public static class Roles
        {
            // PLATFORM scope (cross-tenant)
            public const string PlatformSuperAdmin = "PLATFORM_SUPER_ADMIN";
            public const string PlatformSupport    = "PLATFORM_SUPPORT";
            public const string PlatformAudit      = "PLATFORM_AUDIT";
            public const string UserManagement = "USER_MANAGEMENT";

            // TENANT scope (within a tenant)
            public const string TenantOwner   = "TENANT_OWNER";
            public const string TenantAdmin   = "TENANT_ADMIN";
            public const string TenantManager = "TENANT_MANAGER";
            public const string TenantStaff   = "TENANT_STAFF";
            public const string TenantFinance = "TENANT_FINANCE";

            // CUSTOMER scope (end-users)
            public const string Customer = "CUSTOMER";
            public const string User = "USER"; 
            public const string DefaultUser = "DEFAULT_USER";
        }

        public static class Permissions
        {
            // -------------------------
            // Platform (global) controls
            // -------------------------
            public const string PlatformTenantsManage      = "platform.tenants.manage";
            public const string PlatformSubscriptionsManage= "platform.subscriptions.manage";
            public const string PlatformAuditView          = "platform.audit.view";
            public const string PlatformSystemConfigManage = "platform.system.config.manage";

            // -------------------------
            // Tenant management
            // -------------------------
            public const string TenantView                 = "tenant.view";
            public const string TenantUpdate               = "tenant.update";
            public const string TenantDelete               = "tenant.delete";
            public const string TenantSettingsManage       = "tenant.settings.manage";
            public const string TenantBrandingManage       = "tenant.branding.manage";
            public const string TenantUsersInvite          = "tenant.users.invite";
            public const string TenantUsersManage          = "tenant.users.manage";
            public const string TenantRolesManage          = "tenant.roles.manage";
            public const string TenantPermissionsManage    = "tenant.permissions.manage";

            // -------------------------
            // Resources (courts, rooms, cars, seats)
            // -------------------------
            public const string ResourceCreate             = "resource.create";
            public const string ResourceView               = "resource.view";
            public const string ResourceUpdate             = "resource.update";
            public const string ResourceDelete             = "resource.delete";
            public const string ResourcePricingManage      = "resource.pricing.manage";
            public const string ResourceAvailabilityManage = "resource.availability.manage";

            // -------------------------
            // Bookings
            // -------------------------
            public const string BookingCreate              = "booking.create";
            public const string BookingViewOwn             = "booking.view.own";
            public const string BookingCancelOwn           = "booking.cancel.own";
            public const string BookingRescheduleOwn       = "booking.reschedule.own";

            public const string BookingViewAll             = "booking.view.all";
            public const string BookingManageAll           = "booking.manage.all"; // confirm/cancel/reschedule for tenant staff
            public const string BookingRefundRequest       = "booking.refund.request";

            // -------------------------
            // Payments
            // -------------------------
            public const string PaymentInitiate            = "payment.initiate";
            public const string PaymentViewOwn             = "payment.view.own";

            public const string PaymentViewAll             = "payment.view.all";
            public const string PaymentRefund              = "payment.refund";
            public const string PaymentPayoutManage        = "payment.payout.manage";

            // -------------------------
            // Files / Media
            // -------------------------
            public const string FileUpload                 = "file.upload";
            public const string FileView                   = "file.view";
            public const string FileDelete                 = "file.delete";

            // -------------------------
            // Profile / session
            // -------------------------
            public const string ProfileView                = "profile.view";
            public const string ProfileUpdate              = "profile.update";
            public const string DeviceManage               = "device.manage";
            
            //Customer scope
            
            
            
        }

        public static class PermissionGroups
        {
            public static readonly Dictionary<string, List<string>> Groups = new()
            {
                ["Platform"] = new()
                {
                    Permissions.PlatformTenantsManage,
                    Permissions.PlatformSubscriptionsManage,
                    Permissions.PlatformAuditView,
                    Permissions.PlatformSystemConfigManage
                },

                ["Tenant Admin"] = new()
                {
                    Permissions.TenantView,
                    Permissions.TenantUpdate,
                    Permissions.TenantSettingsManage,
                    Permissions.TenantBrandingManage,
                    Permissions.TenantUsersInvite,
                    Permissions.TenantUsersManage,
                    Permissions.TenantRolesManage,
                    Permissions.TenantPermissionsManage
                },

                ["Resources"] = new()
                {
                    Permissions.ResourceCreate,
                    Permissions.ResourceView,
                    Permissions.ResourceUpdate,
                    Permissions.ResourceDelete,
                    Permissions.ResourcePricingManage,
                    Permissions.ResourceAvailabilityManage
                },

                ["Bookings"] = new()
                {
                    Permissions.BookingCreate,
                    Permissions.BookingViewOwn,
                    Permissions.BookingCancelOwn,
                    Permissions.BookingRescheduleOwn,
                    Permissions.BookingViewAll,
                    Permissions.BookingManageAll,
                    Permissions.BookingRefundRequest
                },

                ["Payments"] = new()
                {
                    Permissions.PaymentInitiate,
                    Permissions.PaymentViewOwn,
                    Permissions.PaymentViewAll,
                    Permissions.PaymentRefund,
                    Permissions.PaymentPayoutManage
                },

                ["Files"] = new()
                {
                    Permissions.FileUpload,
                    Permissions.FileView,
                    Permissions.FileDelete
                },

                ["Profile"] = new()
                {
                    Permissions.ProfileView,
                    Permissions.ProfileUpdate,
                    Permissions.DeviceManage
                }
            };
        }

        public static class RolePermissions
        {
            // PLATFORM
            public static readonly List<string> PlatformSuperAdmin = new()
            {
                Permissions.PlatformTenantsManage,
                Permissions.PlatformSubscriptionsManage,
                Permissions.PlatformAuditView,
                Permissions.PlatformSystemConfigManage,

                // optional: also allow tenant-level everything across tenants (if you want)
                Permissions.TenantSettingsManage,
                Permissions.TenantUsersManage,
                Permissions.TenantRolesManage,
                Permissions.TenantPermissionsManage,
                Permissions.ResourceAvailabilityManage,
                Permissions.BookingManageAll,
                Permissions.PaymentRefund,
                Permissions.PaymentPayoutManage,
                Permissions.FileDelete
            };

            public static readonly List<string> PlatformSupport = new()
            {
                Permissions.PlatformTenantsManage,
                Permissions.PlatformSubscriptionsManage,
                Permissions.PlatformAuditView,
                Permissions.BookingViewAll,
                Permissions.PaymentViewAll
            };

            public static readonly List<string> PlatformAudit = new()
            {
                Permissions.PlatformAuditView,
                Permissions.BookingViewAll,
                Permissions.PaymentViewAll
            };

            // TENANT
            public static readonly List<string> TenantOwner = new()
            {
                Permissions.TenantView,
                Permissions.TenantUpdate,
                Permissions.TenantSettingsManage,
                Permissions.TenantBrandingManage,
                Permissions.TenantUsersInvite,
                Permissions.TenantUsersManage,
                Permissions.TenantRolesManage,
                Permissions.TenantPermissionsManage,

                Permissions.ResourceCreate,
                Permissions.ResourceView,
                Permissions.ResourceUpdate,
                Permissions.ResourceDelete,
                Permissions.ResourcePricingManage,
                Permissions.ResourceAvailabilityManage,

                Permissions.BookingViewAll,
                Permissions.BookingManageAll,

                Permissions.PaymentViewAll,
                Permissions.PaymentRefund,
                Permissions.PaymentPayoutManage,

                Permissions.FileUpload,
                Permissions.FileView,
                Permissions.FileDelete,

                Permissions.ProfileView,
                Permissions.ProfileUpdate
            };

            public static readonly List<string> TenantAdmin = new(TenantOwner); // you can reduce later if you want

            public static readonly List<string> TenantManager = new()
            {
                Permissions.ResourceView,
                Permissions.ResourceUpdate,
                Permissions.ResourceAvailabilityManage,

                Permissions.BookingViewAll,
                Permissions.BookingManageAll,

                Permissions.PaymentViewAll,

                Permissions.FileUpload,
                Permissions.FileView,

                Permissions.ProfileView,
                Permissions.ProfileUpdate
            };

            public static readonly List<string> TenantStaff = new()
            {
                Permissions.ResourceView,
                Permissions.BookingViewAll,
                Permissions.BookingManageAll,
                Permissions.FileUpload,
                Permissions.FileView,
                Permissions.ProfileView,
                Permissions.ProfileUpdate
            };

            public static readonly List<string> TenantFinance = new()
            {
                Permissions.PaymentViewAll,
                Permissions.PaymentRefund,
                Permissions.PaymentPayoutManage,
                Permissions.BookingViewAll,
                Permissions.ProfileView
            };

            // CUSTOMER
            public static readonly List<string> Customer = new()
            {
                Permissions.BookingCreate,
                Permissions.BookingViewOwn,
                Permissions.BookingCancelOwn,
                Permissions.BookingRescheduleOwn,
                Permissions.PaymentInitiate,
                Permissions.PaymentViewOwn,
                Permissions.FileUpload,  // optional, if customers can upload docs (IDs, etc)
                Permissions.FileView,
                Permissions.ProfileView,
                Permissions.ProfileUpdate,
                Permissions.DeviceManage
            };
        }

        #endregion
    }
}
