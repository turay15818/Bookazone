using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookazone.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VehicleOrders",
                schema: "vehicles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FkVehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    FkTenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FkCustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceType = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PricingUnit = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    StartUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PickupAddress = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    DropoffAddress = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    PickupLatitude = table.Column<double>(type: "double precision", nullable: true),
                    PickupLongitude = table.Column<double>(type: "double precision", nullable: true),
                    DropoffLatitude = table.Column<double>(type: "double precision", nullable: true),
                    DropoffLongitude = table.Column<double>(type: "double precision", nullable: true),
                    CargoDescription = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CargoWeightTons = table.Column<decimal>(type: "numeric", nullable: true),
                    CargoVolumeCubicMeters = table.Column<decimal>(type: "numeric", nullable: true),
                    DriverRequested = table.Column<bool>(type: "boolean", nullable: false),
                    ContactName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ContactEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ContactPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DateDeleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FkUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleOrders_Tenants_FkTenantId",
                        column: x => x.FkTenantId,
                        principalSchema: "tenants",
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VehicleOrders_Users_FkCustomerId",
                        column: x => x.FkCustomerId,
                        principalSchema: "profile",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VehicleOrders_Users_FkUserId",
                        column: x => x.FkUserId,
                        principalSchema: "profile",
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VehicleOrders_Vehicles_FkVehicleId",
                        column: x => x.FkVehicleId,
                        principalSchema: "vehicles",
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VehicleOrders_FkCustomerId",
                schema: "vehicles",
                table: "VehicleOrders",
                column: "FkCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleOrders_FkTenantId",
                schema: "vehicles",
                table: "VehicleOrders",
                column: "FkTenantId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleOrders_FkUserId",
                schema: "vehicles",
                table: "VehicleOrders",
                column: "FkUserId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleOrders_FkVehicleId",
                schema: "vehicles",
                table: "VehicleOrders",
                column: "FkVehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleOrders_Status_ServiceType",
                schema: "vehicles",
                table: "VehicleOrders",
                columns: new[] { "Status", "ServiceType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VehicleOrders",
                schema: "vehicles");
        }
    }
}
