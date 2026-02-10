#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Bookazone.Infrastructure.Persistence.Seed;

public partial class SeedRolesAndPermissions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // -----------------------
        // 1) Seed Roles
        // -----------------------
        var roles = new[]
        {
            // Platform roles
            "PLATFORM_SUPER_ADMIN",
            "PLATFORM_SUPPORT",
            "PLATFORM_AUDIT",

            // Tenant roles
            "TENANT_OWNER",
            "TENANT_ADMIN",
            "TENANT_MANAGER",
            "TENANT_STAFF",
            "TENANT_FINANCE",

            // Customer
            "CUSTOMER"
        };

        // Role IDs (stable)
        var roleIds = new (string Name, Guid Id)[]
        {
            ("PLATFORM_SUPER_ADMIN", Guid.Parse("f2c9b4d1-6b52-4a9e-9c21-1b47b8f4a901")),
            ("PLATFORM_SUPPORT",     Guid.Parse("8a1d3d7e-5b8e-4c3c-b9a7-6a34c9a2e402")),
            ("PLATFORM_AUDIT",       Guid.Parse("c6d74a22-4f9a-46f6-9a88-52e91f14c6d3")),

            ("TENANT_OWNER",         Guid.Parse("1f9c2b7d-83ef-4a74-a58e-7a6c6d8c71a1")),
            ("TENANT_ADMIN",         Guid.Parse("b3c5b8d4-4b71-4bb5-9a0a-3e8f7e4b6f92")),
            ("TENANT_MANAGER",       Guid.Parse("5a8a9b74-6a38-4b9b-9fd3-4c57d2a1a903")),
            ("TENANT_STAFF",         Guid.Parse("4e71f9e1-7a44-4fbc-8c9e-5c93b61f4d64")),
            ("TENANT_FINANCE",       Guid.Parse("9b7e1d64-0e4a-4c7a-8c9c-8f2b7d91e0a5")),

            ("CUSTOMER",             Guid.Parse("e81b2b35-1c68-46f0-94d6-93d98d9b27c8")),
        };


        foreach (var r in roleIds)
        {
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Name", "DateCreated", "Active", "Deleted" },
                values: new object[] { r.Id, r.Name, DateTime.UtcNow, true, false }
            );
        }

        // -----------------------
        // 2) Seed Permissions
        // -----------------------
        var permissions = new (string Key, string Group, Guid Id)[]
        {
            // Platform
            ("platform.tenants.manage",       "Platform", Guid.Parse("2c3d9c5e-4d2c-4c0f-bb6f-92bdb6a0e101")),
            ("platform.subscriptions.manage", "Platform", Guid.Parse("a5b7c8e9-2c91-4c3f-b3a7-72d8e8fce102")),
            ("platform.audit.view",           "Platform", Guid.Parse("c7f4e13d-93c1-4e92-9ef5-8a94a6ce3103")),
            ("platform.system.config.manage", "Platform", Guid.Parse("9d7c4a1f-6c2d-4c97-8a91-4e1f7f8b5104")),


            // Tenant Admin
            ("tenant.view",                   "Tenant Admin", Guid.Parse("f4a7b2d1-8c1a-4f21-8c7e-41a6f5b41001")),
            ("tenant.update",                 "Tenant Admin", Guid.Parse("8d2c1e5a-7f4c-4b9a-8c9a-7b5f8a2d1002")),
            ("tenant.settings.manage",        "Tenant Admin", Guid.Parse("3b91c7a2-4f8a-4e61-a5f3-97b6a92e1003")),
            ("tenant.branding.manage",        "Tenant Admin", Guid.Parse("c6f3d82a-1f2c-49a7-9b1a-8f71b5d01004")),
            ("tenant.users.invite",           "Tenant Admin", Guid.Parse("6e82f7a1-2a7b-4bfa-b71d-9f3a6c411005")),
            ("tenant.users.manage",           "Tenant Admin", Guid.Parse("1d4f9e2b-6a8b-4c51-9d9a-92c3eab01006")),
            ("tenant.roles.manage",           "Tenant Admin", Guid.Parse("e8a9d6b1-91a7-47a3-8e92-71d7e8101007")),
            ("tenant.permissions.manage",     "Tenant Admin", Guid.Parse("7b3e9c2a-4f81-4c5a-bf13-3a8c9f501008")),


            // Resources
            ("resource.create",               "Resources", Guid.Parse("5f9e7c2b-3a71-4b41-a7b1-4f91d3a21001")),
            ("resource.view",                 "Resources", Guid.Parse("a1b8c7d9-6f3c-4e2b-9d71-8b7a1c421002")),
            ("resource.update",               "Resources", Guid.Parse("9c7a3d8e-5b21-4c81-8e1f-91b2dca31003")),
            ("resource.delete",               "Resources", Guid.Parse("3a8f6b7e-1d4c-49f2-b3a7-7e6c5f941004")),
            ("resource.pricing.manage",       "Resources", Guid.Parse("d9a3e8c1-72f4-4a9d-b5c2-6b4a1c521005")),
            ("resource.availability.manage",  "Resources", Guid.Parse("7f5d2c91-8a3b-4f7a-9d42-5c7b91a61006")),


            // Bookings
            ("booking.create",                "Bookings", Guid.Parse("2f91a8c4-1c6d-4b7e-a9f3-4c5d6a211001")),
            ("booking.view.own",              "Bookings", Guid.Parse("7a9c5d2e-3b41-49f2-9c81-8e6b7f221002")),
            ("booking.cancel.own",            "Bookings", Guid.Parse("b4c9f1a8-5e7a-4d8b-b6f2-4a7c9e231003")),
            ("booking.reschedule.own",        "Bookings", Guid.Parse("d8c9a6b1-7e4f-4c91-8f52-5b7a8d241004")),
            ("booking.view.all",              "Bookings", Guid.Parse("e1b5a9c7-1f2a-4c6b-b8a7-9c3f8a251005")),
            ("booking.manage.all",            "Bookings", Guid.Parse("9a7e5b8d-4c2a-4f3b-9e71-6a1d8f261006")),
            ("booking.refund.request",        "Bookings", Guid.Parse("6b8f9d1c-2e7a-4c5b-8a93-7e1f5d271007")),


            // Payments
            ("payment.initiate",              "Payments", Guid.Parse("3e7b9f2a-1c8d-4a71-b6f3-5c9d1a311001")),
            ("payment.view.own",              "Payments", Guid.Parse("7d1f9a8e-4b5c-4e7a-9c2a-6b3f8d321002")),
            ("payment.view.all",              "Payments", Guid.Parse("c5a9e1f7-6b2a-4d8c-b3f1-8d9a7e331003")),
            ("payment.refund",                "Payments", Guid.Parse("9f3c1b5d-8a7e-4b92-9c61-2d8f7a341004")),
            ("payment.payout.manage",         "Payments", Guid.Parse("e8b2a6f9-1c5d-4e7a-b9f3-7d1c8a351005")),


            // Files
            ("file.upload",                   "Files", Guid.Parse("2a8d9f7c-5b6e-4c31-b9a1-7e8d3f411001")),
            ("file.view",                     "Files", Guid.Parse("6c5b9e8a-1f3d-4a72-8c91-d2f7a5411002")),
            ("file.delete",                   "Files", Guid.Parse("9f1a7c8e-3b2d-4c5a-b8f6-5d9e3a431003")),

            // Profile
            ("profile.view",                  "Profile", Guid.Parse("b5a9f2d1-8e7c-4c31-9f61-2d7a8e511001")),
            ("profile.update",                "Profile", Guid.Parse("8d7f3a2c-5b1e-4a9c-b6f1-9e5d2a521002")),
            ("device.manage",                 "Profile", Guid.Parse("e2a7b8f5-1c9d-4f6a-9b3e-8d7c5a531003")),

        };

        foreach (var p in permissions)
        {
            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "Name", "Group", "DateCreated", "Active", "Deleted" },
                values: new object[] { p.Id, p.Key, p.Group, DateTime.UtcNow, true, false }
            );
        }

        // -----------------------
        // 3) Seed RolePermissions mappings
        // -----------------------

        Guid R(string name) => Array.Find(roleIds, x => x.Name == name).Id;
        Guid P(string key) => Array.Find(permissions, x => x.Key == key).Id;

        // Helper for inserting mappings
        void AddRolePerm(string role, string permKey)
        {
            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "Id", "RoleId", "PermissionId", "DateCreated", "Active", "Deleted" },
                values: new object[]
                {
                    Guid.NewGuid(), R(role), P(permKey), DateTime.UtcNow, true, false
                }
            );
        }

        // PLATFORM_SUPER_ADMIN = everything (safe approach: grant major ones)
        foreach (var p in permissions)
            AddRolePerm("PLATFORM_SUPER_ADMIN", p.Key);

        // PLATFORM_SUPPORT
        AddRolePerm("PLATFORM_SUPPORT", "platform.tenants.manage");
        AddRolePerm("PLATFORM_SUPPORT", "platform.subscriptions.manage");
        AddRolePerm("PLATFORM_SUPPORT", "platform.audit.view");
        AddRolePerm("PLATFORM_SUPPORT", "booking.view.all");
        AddRolePerm("PLATFORM_SUPPORT", "payment.view.all");

        // PLATFORM_AUDIT
        AddRolePerm("PLATFORM_AUDIT", "platform.audit.view");
        AddRolePerm("PLATFORM_AUDIT", "booking.view.all");
        AddRolePerm("PLATFORM_AUDIT", "payment.view.all");

        // TENANT_OWNER (everything tenant + resources + bookings + payments + files)
        var tenantOwnerPerms = new[]
        {
            "tenant.view","tenant.update","tenant.settings.manage","tenant.branding.manage",
            "tenant.users.invite","tenant.users.manage","tenant.roles.manage","tenant.permissions.manage",

            "resource.create","resource.view","resource.update","resource.delete",
            "resource.pricing.manage","resource.availability.manage",

            "booking.view.all","booking.manage.all",

            "payment.view.all","payment.refund","payment.payout.manage",

            "file.upload","file.view","file.delete",
            "profile.view","profile.update"
        };

        foreach (var key in tenantOwnerPerms)
            AddRolePerm("TENANT_OWNER", key);

        // TENANT_ADMIN (same as owner for now)
        foreach (var key in tenantOwnerPerms)
            AddRolePerm("TENANT_ADMIN", key);

        // TENANT_MANAGER
        foreach (var key in new[]
                 {
                     "resource.view","resource.update","resource.availability.manage",
                     "booking.view.all","booking.manage.all",
                     "payment.view.all",
                     "file.upload","file.view",
                     "profile.view","profile.update"
                 })
            AddRolePerm("TENANT_MANAGER", key);

        // TENANT_STAFF
        foreach (var key in new[]
                 {
                     "resource.view",
                     "booking.view.all","booking.manage.all",
                     "file.upload","file.view",
                     "profile.view","profile.update"
                 })
            AddRolePerm("TENANT_STAFF", key);

        // TENANT_FINANCE
        foreach (var key in new[]
                 {
                     "payment.view.all","payment.refund","payment.payout.manage",
                     "booking.view.all",
                     "profile.view"
                 })
            AddRolePerm("TENANT_FINANCE", key);

        // CUSTOMER
        foreach (var key in new[]
                 {
                     "booking.create","booking.view.own","booking.cancel.own","booking.reschedule.own",
                     "payment.initiate","payment.view.own",
                     "file.upload","file.view",
                     "profile.view","profile.update","device.manage"
                 })
            AddRolePerm("CUSTOMER", key);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Reverse order: RolePermissions -> Permissions -> Roles
        migrationBuilder.Sql("DELETE FROM \"RolePermissions\";");
        migrationBuilder.Sql("DELETE FROM \"Permissions\";");
        migrationBuilder.Sql("DELETE FROM \"Roles\";");
    }
}