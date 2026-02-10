using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookazone.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "vehicles");

            migrationBuilder.CreateTable(
                name: "VehicleMedia",
                schema: "vehicles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FkVehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    FkStoredFileId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCover = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_VehicleMedia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleMedia_StoredFiles_FkStoredFileId",
                        column: x => x.FkStoredFileId,
                        principalTable: "StoredFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VehicleMedia_Users_FkUserId",
                        column: x => x.FkUserId,
                        principalSchema: "profile",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Vehicles",
                schema: "vehicles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FkTenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceType = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Subtitle = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Description = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    Make = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Year = table.Column<int>(type: "integer", nullable: true),
                    Seats = table.Column<int>(type: "integer", nullable: true),
                    Transmission = table.Column<int>(type: "integer", nullable: true),
                    FuelType = table.Column<int>(type: "integer", nullable: true),
                    HasAirConditioning = table.Column<bool>(type: "boolean", nullable: true),
                    HasInsurance = table.Column<bool>(type: "boolean", nullable: true),
                    DeliveryAvailable = table.Column<bool>(type: "boolean", nullable: true),
                    DriverOption = table.Column<int>(type: "integer", nullable: false),
                    FuelPolicy = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    MinBookingDuration = table.Column<int>(type: "integer", nullable: true),
                    MaxBookingDuration = table.Column<int>(type: "integer", nullable: true),
                    BookingDurationUnit = table.Column<int>(type: "integer", nullable: true),
                    MaxLoadTons = table.Column<decimal>(type: "numeric", nullable: true),
                    CargoVolumeCubicMeters = table.Column<decimal>(type: "numeric", nullable: true),
                    BookingMode = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PriceFrom = table.Column<decimal>(type: "numeric", nullable: true),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    FkCoverMediaId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_Vehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vehicles_Tenants_FkTenantId",
                        column: x => x.FkTenantId,
                        principalSchema: "tenants",
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Vehicles_Users_FkUserId",
                        column: x => x.FkUserId,
                        principalSchema: "profile",
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Vehicles_VehicleMedia_FkCoverMediaId",
                        column: x => x.FkCoverMediaId,
                        principalSchema: "vehicles",
                        principalTable: "VehicleMedia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "VehiclePolicies",
                schema: "vehicles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FkVehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Body = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_VehiclePolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehiclePolicies_Users_FkUserId",
                        column: x => x.FkUserId,
                        principalSchema: "profile",
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VehiclePolicies_Vehicles_FkVehicleId",
                        column: x => x.FkVehicleId,
                        principalSchema: "vehicles",
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VehiclePricingRules",
                schema: "vehicles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FkVehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Unit = table.Column<int>(type: "integer", nullable: false),
                    PriceAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    MinQuantity = table.Column<decimal>(type: "numeric", nullable: true),
                    MaxQuantity = table.Column<decimal>(type: "numeric", nullable: true),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_VehiclePricingRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehiclePricingRules_Users_FkUserId",
                        column: x => x.FkUserId,
                        principalSchema: "profile",
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VehiclePricingRules_Vehicles_FkVehicleId",
                        column: x => x.FkVehicleId,
                        principalSchema: "vehicles",
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VehicleSpecs",
                schema: "vehicles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FkVehicleId = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Value = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_VehicleSpecs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VehicleSpecs_Users_FkUserId",
                        column: x => x.FkUserId,
                        principalSchema: "profile",
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VehicleSpecs_Vehicles_FkVehicleId",
                        column: x => x.FkVehicleId,
                        principalSchema: "vehicles",
                        principalTable: "Vehicles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VehicleMedia_FkStoredFileId",
                schema: "vehicles",
                table: "VehicleMedia",
                column: "FkStoredFileId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleMedia_FkUserId",
                schema: "vehicles",
                table: "VehicleMedia",
                column: "FkUserId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleMedia_FkVehicleId_SortOrder",
                schema: "vehicles",
                table: "VehicleMedia",
                columns: new[] { "FkVehicleId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_VehiclePolicies_FkUserId",
                schema: "vehicles",
                table: "VehiclePolicies",
                column: "FkUserId");

            migrationBuilder.CreateIndex(
                name: "IX_VehiclePolicies_FkVehicleId_SortOrder",
                schema: "vehicles",
                table: "VehiclePolicies",
                columns: new[] { "FkVehicleId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_VehiclePricingRules_FkUserId",
                schema: "vehicles",
                table: "VehiclePricingRules",
                column: "FkUserId");

            migrationBuilder.CreateIndex(
                name: "IX_VehiclePricingRules_FkVehicleId",
                schema: "vehicles",
                table: "VehiclePricingRules",
                column: "FkVehicleId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_City",
                schema: "vehicles",
                table: "Vehicles",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_FkCoverMediaId",
                schema: "vehicles",
                table: "Vehicles",
                column: "FkCoverMediaId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_FkTenantId",
                schema: "vehicles",
                table: "Vehicles",
                column: "FkTenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_FkUserId",
                schema: "vehicles",
                table: "Vehicles",
                column: "FkUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehicles_Status_ServiceType",
                schema: "vehicles",
                table: "Vehicles",
                columns: new[] { "Status", "ServiceType" });

            migrationBuilder.CreateIndex(
                name: "IX_VehicleSpecs_FkUserId",
                schema: "vehicles",
                table: "VehicleSpecs",
                column: "FkUserId");

            migrationBuilder.CreateIndex(
                name: "IX_VehicleSpecs_FkVehicleId_SortOrder",
                schema: "vehicles",
                table: "VehicleSpecs",
                columns: new[] { "FkVehicleId", "SortOrder" });

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleMedia_Vehicles_FkVehicleId",
                schema: "vehicles",
                table: "VehicleMedia",
                column: "FkVehicleId",
                principalSchema: "vehicles",
                principalTable: "Vehicles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleMedia_Vehicles_FkVehicleId",
                schema: "vehicles",
                table: "VehicleMedia");

            migrationBuilder.DropTable(
                name: "VehiclePolicies",
                schema: "vehicles");

            migrationBuilder.DropTable(
                name: "VehiclePricingRules",
                schema: "vehicles");

            migrationBuilder.DropTable(
                name: "VehicleSpecs",
                schema: "vehicles");

            migrationBuilder.DropTable(
                name: "Vehicles",
                schema: "vehicles");

            migrationBuilder.DropTable(
                name: "VehicleMedia",
                schema: "vehicles");
        }
    }
}
