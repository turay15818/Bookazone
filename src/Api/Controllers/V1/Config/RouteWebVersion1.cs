namespace Bookazone.Api.Controllers.V1.Config;

public static class RouteWebVersion1
{
    private const string PrefixWeb = "/api/web/v1";

    public static class Public
    {
        public const string Base = PrefixWeb + "/public";
        public const string Test = PrefixWeb + "test";

        public static class Booking
        {
            private const string PrefixBooking = Base + "/booking";

            public static class Sports
            {
                public const string Base = PrefixBooking + "/sports";
                public const string Types = "types";
                public const string Resources = "resources";
                public const string Resource = "resources/{id}";
                public const string Availability = "resources/{id}/availability";
                public const string Slots = "resources/{id}/slots";
            }

            public static class Tenants
            {
                public const string Base = PrefixBooking + "/tenants";
                public const string All = "all";
            }
        }

        public static class Events
        {
            private const string PrefixEvents = RouteWebVersion1.Public.Base + "/events";
            public const string Base = PrefixEvents;
            public const string Categories = "categories";
            public const string All = "all";
            public const string Find = "{id}";
            public const string Similar = "{id}/similar";
        }

        public static class Vehicles
        {
            private const string PrefixVehicles = RouteWebVersion1.Public.Base + "/vehicles";
            public const string Base = PrefixVehicles;
            public const string All = "all";
            public const string Find = "{id}";
        }

        public static class Reviews
        {
            private const string PrefixReviews = RouteWebVersion1.Public.Base + "/reviews";
            public const string Base = PrefixReviews;
            public const string All = "all";
            public const string Summary = "summary";
        }
    }

    public static class Security
    {
        private const string PrefixSecurity = PrefixWeb + "";

        public static class Auth
        {
            public const string Base = PrefixSecurity + "/auth";
            public const string CheckUsername = "checkusername";
            public const string Login = "login";
            public const string Google = "google";
            public const string LoginBiometric = "login/biometric";
            public const string InviteAccept = "invite/accept";
            public const string InviteSetPassword = "invite/setpassword";
            public const string RefreshToken = "refresh/token";
            public const string Logout = "logout";
            public const string LogoutAll = "logout/all";
            public const string OtpValidate = "otp/validate";
            public const string DeviceValidate = "device/validate";
            public const string ResendVerification = "resend/verification";
            public const string VerifyReset = "verify/reset";
            public const string IssueToken = "token/issue";
            public const string PasswordReset = "password/reset";
            public const string PasswordResetOtp = "password/reset/otp";
            public const string PasswordResetValidate = "password/reset/validate";
            public const string UpgradeCheck = "upgrade/check";
        }
    }

    public static class Secure
    {
        private const string PrefixSecure = PrefixWeb + "/secure";
        public static class Home
        {
            public const string Base = PrefixSecure + "/default";
        }

        public static class Device
        {
            public const string Base = PrefixSecure + "/device";
            public const string EmailPhoneCheck = "emailphonecheck";
            public const string Validate = "validate";
            public const string SetBiometric = "setbiometric";
            public const string All = "all";
            public const string Find = "find";
            public const string Delete = "delete";
        }
        
        public static class User
        {
            public const string Base = PrefixSecure + "/user";
            public const string Profile = "profile";
            public const string UserExists = "exists";
            public const string All = "all";
            public const string Find = "find";
            public const string FindByShopId = "find/by/shopid";
            public const string Create = "create";
            public const string Invite = "invite";
            public const string TenantAddUser = "tenant/adduser";
            public const string Update = "update";
            public const string UpdatePassword = "update/password";
            public const string Delete = "delete";
        }
        
        public static class Permission
        {
            public const string Base = PrefixSecure + "/permission";
            public const string All = "all";
            public const string Find = "find";
            public const string FindByName = "find/by/name";
            public const string Create = "create";
            public const string Update = "update";
            public const string Delete = "delete";
            public const string Status = "status";
        }

        public static class Lookup
        {
            public const string Base = PrefixSecure + "/lookup";
            public const string Brand = "brand";
            public const string Category = "category";
            public const string Unit = "unit";
            public const string Product = "product";
            public const string Warehouse = "warehouse";
            public const string Location = "location";
            public const string Supplier = "supplier";
            public const string Customer = "customer";
        }

        public static class CheckList
        {
            public const string Base = PrefixSecure + "/checklist";
            public const string Check = "check";
        }
        public static class Sales
        {
            public const string PrefixSales = PrefixSecure + "/sales";

            public static class SalesOrder
            {
                public const string Base = PrefixSales + "/order";
                public const string All = "all";
                public const string Find = "find";
                public const string Create = "create";
                public const string Payment = "payment";
                public const string Sell = "sell";
                public const string Update = "update";
                public const string Confirm = "confirm";
                public const string Cancel = "cancel";
                public const string Issue = "issue";
            } 
            public static class Pos
            {
                public const string Base = PrefixSales + "/pos";
                public const string Sell = "sell";
            }
            public static class SaleLedger
            {
                public const string Base = PrefixSales + "/saleledger";
                public const string Reconcile = "reconcile";
            }
            public static class Receipt
            {
                public const string Base = PrefixSales + "/receipt";
                public const string Get = "get";
            }
            public static class Promotion
            {
                public const string Base = PrefixSales + "/promotion";
                public const string Create = "create";
                public const string Update = "update";
                public const string All = "all";
                public const string Disable = "disable";
            }
            
            
            public static class Readonly
            {
                public const string Base = PrefixSales + "/readonly";
                public static class SalesSummary
                {
                    public const string Base = Readonly.Base + "/salessummary";
                    public const string Summary = "summary";
                    public const string All = "all";
                }
            }
            
            
        }

        public static class SalesReturn
        {
            public const string PrefixSalesReturn = PrefixSecure + "/salesreturn";

            public static class Return
            {
                public const string Base = PrefixSalesReturn + "/return";
                public const string CreateDraft = "createdraft";
                public const string Confirm = "confirm";
                public const string Cancel = "cancel";
                public const string Complete = "complete";
                public const string Get = "get";

            }
            public static class AdjustmentCalculator
            {
                public const string Base = PrefixSalesReturn + "/adjustmentcalculator";
                public const string Calculate = "calculate";

            }
            public static class ReturnSettlement
            {
                public const string Base = PrefixSalesReturn + "/returnsettlement";
                public const string Settlements = "settlements";
                
            }
        }
        
        public static class Inventory
        {
            public const string PrefixInventory = PrefixSecure + "/inventory";

            public static class Product
            {
                public const string Base = PrefixInventory + "/product";
                public const string All = "all";
                public const string Find = "find";
                public const string FindByName = "find/by/name";
                public const string Create = "create";
                public const string Update = "update";
                public const string Lookup = "lookup";
                public const string Sku = "sku";
                public const string Barcode = "barcode";
                public const string Delete = "delete";
                public const string Overview = "overview";
                public const string Status = "status";
                public const string UploadImage = "upload/image";
                public const string UploadImageMultiple = "upload/image/multiple";
            }

            public static class ProductUnit
            {
                public const string Base = PrefixInventory + "/product/unit";
                public const string All = "all";
                public const string Find = "find";
                public const string FindByName = "find/by/name";
                public const string Create = "create";
                public const string Update = "update";
                public const string Delete = "delete";
                public const string Status = "status";
            }

            public static class ProductUnitConversion
            {
                public const string Base = PrefixInventory + "/product/unit/conversion";
                public const string All = "all";
                public const string Find = "find";
                public const string FindByName = "find/by/name";
                public const string Create = "create";
                public const string Update = "update";
                public const string Delete = "delete";
            }

            public static class Stock
            {
                public const string Base = PrefixInventory + "/stock";
                public const string GetByProduct = "get/by/product";
                public const string Transaction = "transaction";
                public const string Adjust = "adjust";
                public const string Transfer = "transfer";
                public const string Availability = "availability";
                public const string AvailableStock = "availablestock";
                public const string All = "all";
            }

            public static class InventoryValuation
            {
                public const string Base = PrefixInventory + "/iinventoryvaluation";

                public const string Valuation = "valuation";
                public const string InvMovement = "movement";
            }

            public static class Cogs
            {
                public const string Base = PrefixInventory + "/cogs";
                public const string GetCogs = "cogs/get";
                public const string CogsBySale = "cogs/sale";
            }
            public static class InventoryMovement
            {
                public const string Base = PrefixInventory + "/inventorymovement";
                public const string InvMovement = "movement";
            } 
            public static class Reservation
            {
                public const string Base = PrefixInventory + "/reservation";
                public const string Reserve = "reserve";
                public const string Release = "release";
            }
            public static class Brand
            {
                public const string Base = PrefixInventory + "/brand";
                public const string All = "all";
                public const string Find = "find";
                public const string FindByName = "find/by/name";
                public const string Create = "create";
                public const string Update = "update";
                public const string Delete = "delete";
                public const string Status = "status";
            }
            
            public static class ProductPricing
            {
                public const string Base = PrefixInventory + "/productpricing";
                public const string FindByProduct = "find/by/product";
                public const string Create = "create";
                public const string Bulk = "bulk";
                public const string Update = "update";
            }

            public static class TaxRule
            {
                public const string Base = PrefixInventory + "/taxrule";
                public const string Create = "create";
                public const string All = "All";
            }
            public static class DiscountRule
            {
                public const string Base = PrefixInventory + "/discountrule";
                public const string Create = "create";
                public const string All = "All";
            }
            public static class SalePricing
            {
                public const string Base = PrefixInventory + "/salepricing";
                public const string Calculate = "calculate";
            }
            public static class ProductCategory
            {
                public const string Base = PrefixInventory + "/product/category";
                public const string All = "all";
                public const string Find = "find";
                public const string FindByName = "find/by/name";
                public const string Create = "create";
                public const string Update = "update";
                public const string Delete = "delete";
            }

            public static class Movement
            {
                public const string Base = PrefixInventory + "/movement";
                public const string All = "all";
                public const string Find = "find";
                public const string FindByProduct = "find/by/product";
                public const string Create = "create";
                public const string CreateTransaction = "create/transaction";
                public const string CreateTransfer = "create/transfer";
                public const string CreateAdjustment = "create/adjustment";
                public const string History = "history";
                public const string Delete = "delete";
                public const string Update = "update";
            }
            public static class Warehouse
            {
                public const string Base = PrefixInventory + "/warehouse";
                public const string All = "all";
                public const string Find = "find";
                public const string Create = "create";
                public const string Update = "update";
                public const string Delete = "delete";
                public const string Status = "status";
            }

            public static class Location
            {
                public const string Base = PrefixInventory + "/warehouse/location";
                public const string All = "all";
                public const string Find = "find";
                public const string ByWarehouse = "by/warehouse";
                public const string Create = "create";
                public const string Update = "update";
                public const string Delete = "delete";
            }
public static class StockCount
{
    public const string Base = PrefixInventory + "/stockcount";
    public const string All = "all";
    public const string Find = "find";
    public const string Start = "start";
    public const string Finalize = "finalize";
    public const string FindBySession = "find/by/session";
}

public static class Supplier
{
    public const string Base = PrefixInventory + "/supplier";
    public const string All = "all";
    public const string Find = "find";
    public const string Create = "create";
    public const string Update = "update";
    public const string Delete = "delete";  
}
    public static class Customer
        {
            public const string Base = PrefixInventory + "/customer";
            public const string All = "all";
            public const string Find = "find";
            public const string Create = "create";
            public const string Update = "update";
            public const string Delete = "delete";
        }
            
            
            
            
            
        }
        public static class Subscription
        {
            public const string Base = PrefixSecure + "/subscription";
            public const string All = "all";
            public const string Find = "find";
            public const string FindByTenant = "find/by/tenant";
            public const string Create = "create";
            public const string Update = "update";
            public const string Delete = "delete";
            public const string Status = "status";
        } 
        public static class Country
        {
            public const string Base = PrefixSecure + "/country";
            public const string All = "all";
        }
        public static class TenantSubscription
        {
            public const string Base = PrefixSecure + "/tenant/subscription";
            public const string All = "all";
            public const string Find = "find";
            public const string History = "history";
            public const string Upgrade = "upgrade";
            public const string FindByTenant = "find/by/tenant";
            public const string Create = "create";
            public const string Update = "update";
            public const string Delete = "delete";
            public const string Status = "status";
        }
        
        public static class Role
        {
            public const string Base = PrefixSecure + "/role";
            public const string All = "all";
            public const string Find = "find";
            public const string Create = "create";
            public const string Update = "update";
            public const string Delete = "delete";
            public const string Status = "status";
            public const string AssignPermissions = "assign/permissions";
            public const string GetPermissions = "get/permissions";
            public const string AllPermissions = "role/permissions/all";
            public const string PermissionsUpdate = "permissions/update";
            public const string RolesWithPermissions = "role/with-permissions";

        }

        public static class UserRole
        {
            public const string Base = PrefixSecure + "/user/role";
            public const string All = "all";
            public const string Status = "status";
            public const string GetByUser = "get/by/user";
            public const string Assign = "assign";
            public const string Remove = "remove";
            public const string HasRole = "has/role";
        }
        
        public static class UserPermission
        {
            public const string Base = PrefixSecure + "/user/permission";
            public const string GetByUser = "get/by/user";
            public const string Assign = "assign";
            public const string Remove = "remove";
            public const string ClearAll = "clear/all";
        }
        public static class AppConfig
        {
            public const string Base = PrefixSecure + "/app/config";
            public const string All = "all";
            public const string Find = "find";
            public const string FindByName = "find/by/name";
            public const string Create = "create";
            public const string Update = "update";
            public const string Delete = "delete";
            public const string Status = "status";
        }

        public static class Tenant
        {
            public const string Base = PrefixSecure + "/tenant";
            public const string Profile = "profile";
            public const string All = "all";
            public const string Find = "find";
            public const string FindByName = "find/by/name";
            public const string Create = "create";
            public const string Update = "update";
            public const string Delete = "delete";
            public const string Verify = "verify";
            public const string UploadImage = "upload/image";
        }

        public static class BookingConfig
        {
            private const string PrefixBookingConfig = PrefixSecure + "/booking/config";

            public static class Category
            {
                public const string Base = PrefixBookingConfig + "/category";
                public const string All = "all";
                public const string Find = "find";
                public const string Create = "create";
                public const string Update = "update";
                public const string Delete = "delete";
            }

            public static class TenantCategory
            {
                public const string Base = PrefixBookingConfig + "/tenant/category";
                public const string All = "all";
                public const string Bulk = "bulk";
                public const string Upsert = "upsert";
            }

            public static class TenantWorkingHour
            {
                public const string Base = PrefixBookingConfig + "/tenant/workinghour";
                public const string All = "all";
                public const string Bulk = "bulk";
                public const string Upsert = "upsert";
            }

            public static class SportType
            {
                public const string Base = PrefixBookingConfig + "/sport/type";
                public const string All = "all";
                public const string Create = "create";
                public const string Update = "update";
                public const string Delete = "delete";
            }

            public static class SportResource
            {
                public const string Base = PrefixBookingConfig + "/sport/resource";
                public const string ByTenant = "by/tenant";
                public const string ByType = "by/type";
                public const string Create = "create";
                public const string Update = "update";
                public const string Delete = "delete";
            }

            public static class SportResourceAvailability
            {
                public const string Base = PrefixBookingConfig + "/sport/resource/availability";
                public const string All = "all";
                public const string Bulk = "bulk";
                public const string Upsert = "upsert";
            }

            public static class SportResourceBlackout
            {
                public const string Base = PrefixBookingConfig + "/sport/resource/blackout";
                public const string All = "all";
                public const string Create = "create";
                public const string Delete = "delete";
            }

            public static class SportResourcePricing
            {
                public const string Base = PrefixBookingConfig + "/sport/resource/pricing";
                public const string All = "all";
                public const string Create = "create";
                public const string Update = "update";
                public const string Delete = "delete";
                public const string Bulk = "bulk";
            }
        }

        public static class Booking
        {
            public const string Base = PrefixSecure + "/booking";
            public const string Create = "create";
            public const string Update = "update";
            public const string Validate = "validate";
            public const string Approve = "approve";
            public const string Decline = "decline";
            public const string Cancel = "cancel";
            public const string Reschedule = "reschedule";
            public const string TenantList = "tenant";
            public const string CustomerList = "customer";
        }

        public static class Events
        {
            public const string Base = PrefixSecure + "/events";
            public const string All = "all";
            public const string Find = "find";
            public const string Create = "create";
            public const string Update = "update";
            public const string Publish = "publish";
            public const string Cancel = "cancel";
            public const string Delete = "delete";
            public const string Venue = "{eventId}/venue";
            public const string TicketTypesBulk = "{eventId}/tickets/bulk";
            public const string ScheduleBulk = "{eventId}/schedule/bulk";
            public const string PoliciesBulk = "{eventId}/policies/bulk";
            public const string MediaBulk = "{eventId}/media/bulk";

            public static class Orders
            {
                public const string Base = PrefixSecure + "/events/order";
                public const string Create = "create";
                public const string Confirm = "confirm";
                public const string Cancel = "cancel";
                public const string Find = "find";
                public const string Customer = "customer";
                public const string Tenant = "tenant";
            }

            public static class Categories
            {
                public const string Base = PrefixSecure + "/events/category";
                public const string All = "all";
                public const string Find = "find";
                public const string Create = "create";
                public const string Update = "update";
                public const string Delete = "delete";
            }

            public static class Subscriptions
            {
                public const string Base = PrefixSecure + "/events/subscription";
                public const string My = "my";
                public const string Upsert = "upsert";
                public const string Bulk = "bulk";
            }
        }

        public static class Vehicles
        {
            public const string Base = PrefixSecure + "/vehicles";
            public const string All = "all";
            public const string Find = "find";
            public const string Create = "create";
            public const string Update = "update";
            public const string Publish = "publish";
            public const string Status = "status";
            public const string Delete = "delete";
            public const string PricingBulk = "{vehicleId}/pricing/bulk";
            public const string SpecsBulk = "{vehicleId}/specs/bulk";
            public const string PoliciesBulk = "{vehicleId}/policies/bulk";
            public const string MediaBulk = "{vehicleId}/media/bulk";
            public const string MediaUpload = "{vehicleId}/media/upload";
        }

        public static class VehicleOrders
        {
            public const string Base = PrefixSecure + "/vehicles/order";
            public const string Create = "create";
            public const string Find = "find";
            public const string Customer = "customer";
            public const string Tenant = "tenant";
            public const string Status = "status";
            public const string Cancel = "cancel";
        }
        
        public static class Reviews
        {
            public const string Base = PrefixSecure + "/reviews";
            public const string Create = "create";
            public const string Find = "find";
            public const string Customer = "customer";
            public const string Tenant = "tenant";
            public const string Status = "status";
            public const string Reply = "reply";
        }
        public static class Shop
        {
            public const string Base = PrefixSecure + "/shop";
            public const string All = "all";
            public const string Find = "find";
            public const string Create = "create";
            public const string Update = "update";
            public const string Delete = "delete";
            public const string Details = "details";
        }
        public static class UserShop
        {
            public const string Base = PrefixSecure + "/user/shop";
            public const string All = "all";
            public const string GetByUser = "get/by/user";
            public const string GetByShop = "get/by/shop";
            public const string Assign = "assign";
            public const string Remove = "remove";
        }
        public static class Notification
        {
            public const string Base = PrefixSecure + "/notification";
            public const string RegisterToken = "register/token";
            public const string TestNotification = "test/notification";
        }
        public static class Advert
        {
            public const string Base = PrefixSecure + "/advert";
            public const string All = "all";
            public const string Find = "find";
        }

    }
}
