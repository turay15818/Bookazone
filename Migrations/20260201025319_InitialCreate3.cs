using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookazone.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "tenants",
                table: "UserTenants");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "profile",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "UserRole");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "UserPermission");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "profile",
                table: "UserPasswords");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "tenants",
                table: "TenantSubscriptions");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "tenants",
                table: "TenantSettings");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "tenants",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "SubscriptionPlan");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "profile",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "profile",
                table: "RolePermission");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "profile",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "profile",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "profile",
                table: "PermissionGroup");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "tenants",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "profile",
                table: "OneTimePasswords");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "profile",
                table: "Devices");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "profile",
                table: "AuthProviders");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                schema: "audit",
                table: "AuditLog");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "AppConfig");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RowVersion",
                schema: "tenants",
                table: "UserTenants",
                type: "integer",
                rowVersion: true,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RowVersion",
                schema: "profile",
                table: "Users",
                type: "integer",
                rowVersion: true,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RowVersion",
                table: "UserRole",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RowVersion",
                table: "UserPermission",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RowVersion",
                schema: "profile",
                table: "UserPasswords",
                type: "integer",
                rowVersion: true,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RowVersion",
                schema: "tenants",
                table: "TenantSubscriptions",
                type: "integer",
                rowVersion: true,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RowVersion",
                schema: "tenants",
                table: "TenantSettings",
                type: "integer",
                rowVersion: true,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RowVersion",
                schema: "tenants",
                table: "Tenants",
                type: "integer",
                rowVersion: true,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RowVersion",
                table: "SubscriptionPlan",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RowVersion",
                schema: "profile",
                table: "Roles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RowVersion",
                schema: "profile",
                table: "RolePermission",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RowVersion",
                schema: "profile",
                table: "RefreshTokens",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RowVersion",
                schema: "profile",
                table: "Permissions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RowVersion",
                schema: "profile",
                table: "PermissionGroup",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RowVersion",
                schema: "tenants",
                table: "Payments",
                type: "integer",
                rowVersion: true,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RowVersion",
                schema: "profile",
                table: "OneTimePasswords",
                type: "integer",
                rowVersion: true,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RowVersion",
                schema: "profile",
                table: "Devices",
                type: "integer",
                rowVersion: true,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RowVersion",
                schema: "profile",
                table: "AuthProviders",
                type: "integer",
                rowVersion: true,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RowVersion",
                schema: "audit",
                table: "AuditLog",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RowVersion",
                table: "AppConfig",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
