using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookazone.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate12 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "equipment");

            migrationBuilder.CreateTable(
                name: "Equipment",
                schema: "equipment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FkTenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Subtitle = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Description = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    UnitsAvailable = table.Column<int>(type: "integer", nullable: false),
                    DeliveryAvailable = table.Column<bool>(type: "boolean", nullable: true),
                    PickupAvailable = table.Column<bool>(type: "boolean", nullable: true),
                    MinBookingDuration = table.Column<int>(type: "integer", nullable: true),
                    MaxBookingDuration = table.Column<int>(type: "integer", nullable: true),
                    BookingDurationUnit = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_Equipment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Equipment_Tenants_FkTenantId",
                        column: x => x.FkTenantId,
                        principalSchema: "tenants",
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Equipment_Users_FkUserId",
                        column: x => x.FkUserId,
                        principalSchema: "profile",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EquipmentMedia",
                schema: "equipment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FkEquipmentId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_EquipmentMedia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EquipmentMedia_Equipment_FkEquipmentId",
                        column: x => x.FkEquipmentId,
                        principalSchema: "equipment",
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EquipmentMedia_StoredFiles_FkStoredFileId",
                        column: x => x.FkStoredFileId,
                        principalTable: "StoredFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EquipmentMedia_Users_FkUserId",
                        column: x => x.FkUserId,
                        principalSchema: "profile",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EquipmentOrders",
                schema: "equipment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FkEquipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    FkTenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FkCustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PricingUnit = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric", nullable: false),
                    UnitsRequested = table.Column<int>(type: "integer", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    StartUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeliveryAddress = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    DeliveryLatitude = table.Column<double>(type: "double precision", nullable: true),
                    DeliveryLongitude = table.Column<double>(type: "double precision", nullable: true),
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
                    table.PrimaryKey("PK_EquipmentOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EquipmentOrders_Equipment_FkEquipmentId",
                        column: x => x.FkEquipmentId,
                        principalSchema: "equipment",
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EquipmentOrders_Tenants_FkTenantId",
                        column: x => x.FkTenantId,
                        principalSchema: "tenants",
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EquipmentOrders_Users_FkCustomerId",
                        column: x => x.FkCustomerId,
                        principalSchema: "profile",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EquipmentOrders_Users_FkUserId",
                        column: x => x.FkUserId,
                        principalSchema: "profile",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EquipmentPolicies",
                schema: "equipment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FkEquipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Body = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
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
                    table.PrimaryKey("PK_EquipmentPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EquipmentPolicies_Equipment_FkEquipmentId",
                        column: x => x.FkEquipmentId,
                        principalSchema: "equipment",
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EquipmentPolicies_Users_FkUserId",
                        column: x => x.FkUserId,
                        principalSchema: "profile",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EquipmentPricingRules",
                schema: "equipment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FkEquipmentId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_EquipmentPricingRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EquipmentPricingRules_Equipment_FkEquipmentId",
                        column: x => x.FkEquipmentId,
                        principalSchema: "equipment",
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EquipmentPricingRules_Users_FkUserId",
                        column: x => x.FkUserId,
                        principalSchema: "profile",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EquipmentSpecs",
                schema: "equipment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FkEquipmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
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
                    table.PrimaryKey("PK_EquipmentSpecs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EquipmentSpecs_Equipment_FkEquipmentId",
                        column: x => x.FkEquipmentId,
                        principalSchema: "equipment",
                        principalTable: "Equipment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EquipmentSpecs_Users_FkUserId",
                        column: x => x.FkUserId,
                        principalSchema: "profile",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_Category",
                schema: "equipment",
                table: "Equipment",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_City",
                schema: "equipment",
                table: "Equipment",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_FkCoverMediaId",
                schema: "equipment",
                table: "Equipment",
                column: "FkCoverMediaId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_FkTenantId",
                schema: "equipment",
                table: "Equipment",
                column: "FkTenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_FkUserId",
                schema: "equipment",
                table: "Equipment",
                column: "FkUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Equipment_Status",
                schema: "equipment",
                table: "Equipment",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentMedia_FkEquipmentId_SortOrder",
                schema: "equipment",
                table: "EquipmentMedia",
                columns: new[] { "FkEquipmentId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentMedia_FkStoredFileId",
                schema: "equipment",
                table: "EquipmentMedia",
                column: "FkStoredFileId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentMedia_FkUserId",
                schema: "equipment",
                table: "EquipmentMedia",
                column: "FkUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentOrders_FkCustomerId",
                schema: "equipment",
                table: "EquipmentOrders",
                column: "FkCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentOrders_FkEquipmentId",
                schema: "equipment",
                table: "EquipmentOrders",
                column: "FkEquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentOrders_FkTenantId",
                schema: "equipment",
                table: "EquipmentOrders",
                column: "FkTenantId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentOrders_FkUserId",
                schema: "equipment",
                table: "EquipmentOrders",
                column: "FkUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentOrders_Status",
                schema: "equipment",
                table: "EquipmentOrders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentPolicies_FkEquipmentId_SortOrder",
                schema: "equipment",
                table: "EquipmentPolicies",
                columns: new[] { "FkEquipmentId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentPolicies_FkUserId",
                schema: "equipment",
                table: "EquipmentPolicies",
                column: "FkUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentPricingRules_FkEquipmentId",
                schema: "equipment",
                table: "EquipmentPricingRules",
                column: "FkEquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentPricingRules_FkEquipmentId_Unit",
                schema: "equipment",
                table: "EquipmentPricingRules",
                columns: new[] { "FkEquipmentId", "Unit" });

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentPricingRules_FkUserId",
                schema: "equipment",
                table: "EquipmentPricingRules",
                column: "FkUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentSpecs_FkEquipmentId_SortOrder",
                schema: "equipment",
                table: "EquipmentSpecs",
                columns: new[] { "FkEquipmentId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_EquipmentSpecs_FkUserId",
                schema: "equipment",
                table: "EquipmentSpecs",
                column: "FkUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Equipment_EquipmentMedia_FkCoverMediaId",
                schema: "equipment",
                table: "Equipment",
                column: "FkCoverMediaId",
                principalSchema: "equipment",
                principalTable: "EquipmentMedia",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Equipment_EquipmentMedia_FkCoverMediaId",
                schema: "equipment",
                table: "Equipment");

            migrationBuilder.DropTable(
                name: "EquipmentOrders",
                schema: "equipment");

            migrationBuilder.DropTable(
                name: "EquipmentPolicies",
                schema: "equipment");

            migrationBuilder.DropTable(
                name: "EquipmentPricingRules",
                schema: "equipment");

            migrationBuilder.DropTable(
                name: "EquipmentSpecs",
                schema: "equipment");

            migrationBuilder.DropTable(
                name: "EquipmentMedia",
                schema: "equipment");

            migrationBuilder.DropTable(
                name: "Equipment",
                schema: "equipment");
        }
    }
}
