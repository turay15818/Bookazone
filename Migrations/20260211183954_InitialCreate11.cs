using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookazone.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "rentals");

            migrationBuilder.EnsureSchema(
                name: "reviews");

            migrationBuilder.CreateTable(
                name: "Reviews",
                schema: "reviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FkTenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FkCustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetType = table.Column<int>(type: "integer", nullable: false),
                    TargetId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Body = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    ModerationNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Reply = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RepliedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReplyByUserId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.CheckConstraint("CK_Reviews_Rating", "\"Rating\" >= 1 AND \"Rating\" <= 5");
                    table.ForeignKey(
                        name: "FK_Reviews_Tenants_FkTenantId",
                        column: x => x.FkTenantId,
                        principalSchema: "tenants",
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reviews_Users_FkCustomerId",
                        column: x => x.FkCustomerId,
                        principalSchema: "profile",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reviews_Users_FkUserId",
                        column: x => x.FkUserId,
                        principalSchema: "profile",
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Reviews_Users_ReplyByUserId",
                        column: x => x.ReplyByUserId,
                        principalSchema: "profile",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReviewAspectRatings",
                schema: "reviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FkReviewId = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DateDeleted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewAspectRatings", x => x.Id);
                    table.CheckConstraint("CK_ReviewAspectRatings_Score", "\"Score\" >= 1 AND \"Score\" <= 5");
                    table.ForeignKey(
                        name: "FK_ReviewAspectRatings_Reviews_FkReviewId",
                        column: x => x.FkReviewId,
                        principalSchema: "reviews",
                        principalTable: "Reviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RentalMedia",
                schema: "rentals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FkRentalId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_RentalMedia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RentalMedia_StoredFiles_FkStoredFileId",
                        column: x => x.FkStoredFileId,
                        principalTable: "StoredFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RentalMedia_Users_FkUserId",
                        column: x => x.FkUserId,
                        principalSchema: "profile",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Rentals",
                schema: "rentals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FkTenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Subtitle = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Description = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
                    Capacity = table.Column<int>(type: "integer", nullable: true),
                    Bedrooms = table.Column<int>(type: "integer", nullable: true),
                    Bathrooms = table.Column<int>(type: "integer", nullable: true),
                    FloorAreaSquareMeters = table.Column<decimal>(type: "numeric", nullable: true),
                    UnitsAvailable = table.Column<int>(type: "integer", nullable: false),
                    HasToilet = table.Column<bool>(type: "boolean", nullable: true),
                    HasAirConditioning = table.Column<bool>(type: "boolean", nullable: true),
                    HasFan = table.Column<bool>(type: "boolean", nullable: true),
                    HasSoundSystem = table.Column<bool>(type: "boolean", nullable: true),
                    HasWifi = table.Column<bool>(type: "boolean", nullable: true),
                    HasParking = table.Column<bool>(type: "boolean", nullable: true),
                    MinBookingDuration = table.Column<int>(type: "integer", nullable: true),
                    MaxBookingDuration = table.Column<int>(type: "integer", nullable: true),
                    BookingDurationUnit = table.Column<int>(type: "integer", nullable: true),
                    CheckInTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    CheckOutTime = table.Column<TimeSpan>(type: "interval", nullable: true),
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
                    table.PrimaryKey("PK_Rentals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rentals_RentalMedia_FkCoverMediaId",
                        column: x => x.FkCoverMediaId,
                        principalSchema: "rentals",
                        principalTable: "RentalMedia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Rentals_Tenants_FkTenantId",
                        column: x => x.FkTenantId,
                        principalSchema: "tenants",
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Rentals_Users_FkUserId",
                        column: x => x.FkUserId,
                        principalSchema: "profile",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RentalOrders",
                schema: "rentals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FkRentalId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    GuestCount = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_RentalOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RentalOrders_Rentals_FkRentalId",
                        column: x => x.FkRentalId,
                        principalSchema: "rentals",
                        principalTable: "Rentals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RentalOrders_Tenants_FkTenantId",
                        column: x => x.FkTenantId,
                        principalSchema: "tenants",
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RentalOrders_Users_FkCustomerId",
                        column: x => x.FkCustomerId,
                        principalSchema: "profile",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RentalOrders_Users_FkUserId",
                        column: x => x.FkUserId,
                        principalSchema: "profile",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RentalPolicies",
                schema: "rentals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FkRentalId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
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
                    table.PrimaryKey("PK_RentalPolicies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RentalPolicies_Rentals_FkRentalId",
                        column: x => x.FkRentalId,
                        principalSchema: "rentals",
                        principalTable: "Rentals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RentalPolicies_Users_FkUserId",
                        column: x => x.FkUserId,
                        principalSchema: "profile",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RentalPricingRules",
                schema: "rentals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FkRentalId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_RentalPricingRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RentalPricingRules_Rentals_FkRentalId",
                        column: x => x.FkRentalId,
                        principalSchema: "rentals",
                        principalTable: "Rentals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RentalPricingRules_Users_FkUserId",
                        column: x => x.FkUserId,
                        principalSchema: "profile",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RentalSpecs",
                schema: "rentals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FkRentalId = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
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
                    table.PrimaryKey("PK_RentalSpecs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RentalSpecs_Rentals_FkRentalId",
                        column: x => x.FkRentalId,
                        principalSchema: "rentals",
                        principalTable: "Rentals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RentalSpecs_Users_FkUserId",
                        column: x => x.FkUserId,
                        principalSchema: "profile",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_RentalMedia_FkRentalId_SortOrder",
                schema: "rentals",
                table: "RentalMedia",
                columns: new[] { "FkRentalId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_RentalMedia_FkStoredFileId",
                schema: "rentals",
                table: "RentalMedia",
                column: "FkStoredFileId");

            migrationBuilder.CreateIndex(
                name: "IX_RentalMedia_FkUserId",
                schema: "rentals",
                table: "RentalMedia",
                column: "FkUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RentalOrders_FkCustomerId",
                schema: "rentals",
                table: "RentalOrders",
                column: "FkCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_RentalOrders_FkRentalId",
                schema: "rentals",
                table: "RentalOrders",
                column: "FkRentalId");

            migrationBuilder.CreateIndex(
                name: "IX_RentalOrders_FkTenantId",
                schema: "rentals",
                table: "RentalOrders",
                column: "FkTenantId");

            migrationBuilder.CreateIndex(
                name: "IX_RentalOrders_FkUserId",
                schema: "rentals",
                table: "RentalOrders",
                column: "FkUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RentalOrders_Status",
                schema: "rentals",
                table: "RentalOrders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_RentalPolicies_FkRentalId_SortOrder",
                schema: "rentals",
                table: "RentalPolicies",
                columns: new[] { "FkRentalId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_RentalPolicies_FkUserId",
                schema: "rentals",
                table: "RentalPolicies",
                column: "FkUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RentalPricingRules_FkRentalId",
                schema: "rentals",
                table: "RentalPricingRules",
                column: "FkRentalId");

            migrationBuilder.CreateIndex(
                name: "IX_RentalPricingRules_FkRentalId_Unit",
                schema: "rentals",
                table: "RentalPricingRules",
                columns: new[] { "FkRentalId", "Unit" });

            migrationBuilder.CreateIndex(
                name: "IX_RentalPricingRules_FkUserId",
                schema: "rentals",
                table: "RentalPricingRules",
                column: "FkUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Rentals_City",
                schema: "rentals",
                table: "Rentals",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_Rentals_FkCoverMediaId",
                schema: "rentals",
                table: "Rentals",
                column: "FkCoverMediaId");

            migrationBuilder.CreateIndex(
                name: "IX_Rentals_FkTenantId",
                schema: "rentals",
                table: "Rentals",
                column: "FkTenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Rentals_FkUserId",
                schema: "rentals",
                table: "Rentals",
                column: "FkUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Rentals_Status_Type",
                schema: "rentals",
                table: "Rentals",
                columns: new[] { "Status", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_RentalSpecs_FkRentalId_SortOrder",
                schema: "rentals",
                table: "RentalSpecs",
                columns: new[] { "FkRentalId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_RentalSpecs_FkUserId",
                schema: "rentals",
                table: "RentalSpecs",
                column: "FkUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewAspectRatings_FkReviewId",
                schema: "reviews",
                table: "ReviewAspectRatings",
                column: "FkReviewId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewAspectRatings_FkReviewId_SortOrder",
                schema: "reviews",
                table: "ReviewAspectRatings",
                columns: new[] { "FkReviewId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_FkCustomerId",
                schema: "reviews",
                table: "Reviews",
                column: "FkCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_FkTenantId",
                schema: "reviews",
                table: "Reviews",
                column: "FkTenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_FkUserId",
                schema: "reviews",
                table: "Reviews",
                column: "FkUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ReplyByUserId",
                schema: "reviews",
                table: "Reviews",
                column: "ReplyByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_SourceType_SourceId",
                schema: "reviews",
                table: "Reviews",
                columns: new[] { "SourceType", "SourceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_Status_TargetType",
                schema: "reviews",
                table: "Reviews",
                columns: new[] { "Status", "TargetType" });

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_TargetType_TargetId",
                schema: "reviews",
                table: "Reviews",
                columns: new[] { "TargetType", "TargetId" });

            migrationBuilder.AddForeignKey(
                name: "FK_RentalMedia_Rentals_FkRentalId",
                schema: "rentals",
                table: "RentalMedia",
                column: "FkRentalId",
                principalSchema: "rentals",
                principalTable: "Rentals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RentalMedia_Rentals_FkRentalId",
                schema: "rentals",
                table: "RentalMedia");

            migrationBuilder.DropTable(
                name: "RentalOrders",
                schema: "rentals");

            migrationBuilder.DropTable(
                name: "RentalPolicies",
                schema: "rentals");

            migrationBuilder.DropTable(
                name: "RentalPricingRules",
                schema: "rentals");

            migrationBuilder.DropTable(
                name: "RentalSpecs",
                schema: "rentals");

            migrationBuilder.DropTable(
                name: "ReviewAspectRatings",
                schema: "reviews");

            migrationBuilder.DropTable(
                name: "Reviews",
                schema: "reviews");

            migrationBuilder.DropTable(
                name: "Rentals",
                schema: "rentals");

            migrationBuilder.DropTable(
                name: "RentalMedia",
                schema: "rentals");
        }
    }
}
