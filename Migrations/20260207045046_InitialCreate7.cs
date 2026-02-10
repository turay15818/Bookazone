using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookazone.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "booking");

            migrationBuilder.EnsureSchema(
                name: "sports");

            migrationBuilder.CreateTable(
                name: "BookingCategories",
                schema: "booking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DefaultBookingMode = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_BookingCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingCategories_Users_FkUserId",
                        column: x => x.FkUserId,
                        principalSchema: "profile",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TenantWorkingHours",
                schema: "tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FkTenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    OpenTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    CloseTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    IsClosed = table.Column<bool>(type: "boolean", nullable: false),
                    SlotDurationMinutes = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_TenantWorkingHours", x => x.Id);
                    table.CheckConstraint("CK_TenantWorkingHours_TimeRange", "\"IsClosed\" OR (\"OpenTime\" IS NOT NULL AND \"CloseTime\" IS NOT NULL AND \"OpenTime\" < \"CloseTime\")");
                    table.ForeignKey(
                        name: "FK_TenantWorkingHours_Tenants_FkTenantId",
                        column: x => x.FkTenantId,
                        principalSchema: "tenants",
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TenantWorkingHours_Users_FkUserId",
                        column: x => x.FkUserId,
                        principalSchema: "profile",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TenantBookingCategories",
                schema: "booking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FkTenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FkBookingCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    BookingModeOverride = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_TenantBookingCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantBookingCategories_BookingCategories_FkBookingCategory~",
                        column: x => x.FkBookingCategoryId,
                        principalSchema: "booking",
                        principalTable: "BookingCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TenantBookingCategories_Tenants_FkTenantId",
                        column: x => x.FkTenantId,
                        principalSchema: "tenants",
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TenantBookingCategories_Users_FkUserId",
                        column: x => x.FkUserId,
                        principalSchema: "profile",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TenantSportTypes",
                schema: "sports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FkTenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FkTenantBookingCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    BookingMode = table.Column<int>(type: "integer", nullable: false),
                    MinDurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    MaxDurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    BufferMinutes = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_TenantSportTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantSportTypes_TenantBookingCategories_FkTenantBookingCat~",
                        column: x => x.FkTenantBookingCategoryId,
                        principalSchema: "booking",
                        principalTable: "TenantBookingCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TenantSportTypes_Tenants_FkTenantId",
                        column: x => x.FkTenantId,
                        principalSchema: "tenants",
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TenantSportTypes_Users_FkUserId",
                        column: x => x.FkUserId,
                        principalSchema: "profile",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SportResources",
                schema: "sports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FkTenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FkTenantSportTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Capacity = table.Column<int>(type: "integer", nullable: true),
                    Address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Latitude = table.Column<double>(type: "double precision", nullable: true),
                    Longitude = table.Column<double>(type: "double precision", nullable: true),
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
                    table.PrimaryKey("PK_SportResources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SportResources_TenantSportTypes_FkTenantSportTypeId",
                        column: x => x.FkTenantSportTypeId,
                        principalSchema: "sports",
                        principalTable: "TenantSportTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SportResources_Tenants_FkTenantId",
                        column: x => x.FkTenantId,
                        principalSchema: "tenants",
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SportResources_Users_FkUserId",
                        column: x => x.FkUserId,
                        principalSchema: "profile",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TenantSportMedia",
                schema: "sports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FkTenantSportTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    FkStoredFileId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_TenantSportMedia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantSportMedia_StoredFiles_FkStoredFileId",
                        column: x => x.FkStoredFileId,
                        principalTable: "StoredFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TenantSportMedia_TenantSportTypes_FkTenantSportTypeId",
                        column: x => x.FkTenantSportTypeId,
                        principalSchema: "sports",
                        principalTable: "TenantSportTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TenantSportMedia_Users_FkUserId",
                        column: x => x.FkUserId,
                        principalSchema: "profile",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                schema: "booking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FkTenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FkSportResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    FkCustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    BookingMode = table.Column<int>(type: "integer", nullable: false),
                    StartUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
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
                    table.PrimaryKey("PK_Bookings", x => x.Id);
                    table.CheckConstraint("CK_Bookings_TimeRange", "\"StartUtc\" < \"EndUtc\"");
                    table.ForeignKey(
                        name: "FK_Bookings_SportResources_FkSportResourceId",
                        column: x => x.FkSportResourceId,
                        principalSchema: "sports",
                        principalTable: "SportResources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_Tenants_FkTenantId",
                        column: x => x.FkTenantId,
                        principalSchema: "tenants",
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_Users_FkCustomerId",
                        column: x => x.FkCustomerId,
                        principalSchema: "profile",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_Users_FkUserId",
                        column: x => x.FkUserId,
                        principalSchema: "profile",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SportResourceAvailabilities",
                schema: "sports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FkSportResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    OpenTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    CloseTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    IsClosed = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_SportResourceAvailabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SportResourceAvailabilities_SportResources_FkSportResourceId",
                        column: x => x.FkSportResourceId,
                        principalSchema: "sports",
                        principalTable: "SportResources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SportResourceAvailabilities_Users_FkUserId",
                        column: x => x.FkUserId,
                        principalSchema: "profile",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SportResourceBlackouts",
                schema: "sports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FkSportResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_SportResourceBlackouts", x => x.Id);
                    table.CheckConstraint("CK_SportResourceBlackouts_TimeRange", "\"StartUtc\" < \"EndUtc\"");
                    table.ForeignKey(
                        name: "FK_SportResourceBlackouts_SportResources_FkSportResourceId",
                        column: x => x.FkSportResourceId,
                        principalSchema: "sports",
                        principalTable: "SportResources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SportResourceBlackouts_Users_FkUserId",
                        column: x => x.FkUserId,
                        principalSchema: "profile",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SportResourceMedia",
                schema: "sports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FkSportResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    FkStoredFileId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_SportResourceMedia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SportResourceMedia_SportResources_FkSportResourceId",
                        column: x => x.FkSportResourceId,
                        principalSchema: "sports",
                        principalTable: "SportResources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SportResourceMedia_StoredFiles_FkStoredFileId",
                        column: x => x.FkStoredFileId,
                        principalTable: "StoredFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SportResourceMedia_Users_FkUserId",
                        column: x => x.FkUserId,
                        principalSchema: "profile",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SportResourcePricingRules",
                schema: "sports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FkSportResourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleType = table.Column<int>(type: "integer", nullable: false),
                    PriceAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: true),
                    StartTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    EndTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    MinDurationMinutes = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_SportResourcePricingRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SportResourcePricingRules_SportResources_FkSportResourceId",
                        column: x => x.FkSportResourceId,
                        principalSchema: "sports",
                        principalTable: "SportResources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SportResourcePricingRules_Users_FkUserId",
                        column: x => x.FkUserId,
                        principalSchema: "profile",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BookingApprovals",
                schema: "booking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FkBookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    Decision = table.Column<int>(type: "integer", nullable: false),
                    DecisionAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_BookingApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BookingApprovals_Bookings_FkBookingId",
                        column: x => x.FkBookingId,
                        principalSchema: "booking",
                        principalTable: "Bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookingApprovals_Users_FkUserId",
                        column: x => x.FkUserId,
                        principalSchema: "profile",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookingApprovals_FkBookingId",
                schema: "booking",
                table: "BookingApprovals",
                column: "FkBookingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookingApprovals_FkUserId",
                schema: "booking",
                table: "BookingApprovals",
                column: "FkUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingCategories_Code",
                schema: "booking",
                table: "BookingCategories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookingCategories_FkUserId",
                schema: "booking",
                table: "BookingCategories",
                column: "FkUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BookingCategories_Name",
                schema: "booking",
                table: "BookingCategories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_FkCustomerId",
                schema: "booking",
                table: "Bookings",
                column: "FkCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_FkSportResourceId_StartUtc_EndUtc_Status",
                schema: "booking",
                table: "Bookings",
                columns: new[] { "FkSportResourceId", "StartUtc", "EndUtc", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_FkTenantId",
                schema: "booking",
                table: "Bookings",
                column: "FkTenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_FkTenantId_Status_StartUtc",
                schema: "booking",
                table: "Bookings",
                columns: new[] { "FkTenantId", "Status", "StartUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_FkUserId",
                schema: "booking",
                table: "Bookings",
                column: "FkUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SportResourceAvailabilities_FkSportResourceId_DayOfWeek",
                schema: "sports",
                table: "SportResourceAvailabilities",
                columns: new[] { "FkSportResourceId", "DayOfWeek" });

            migrationBuilder.CreateIndex(
                name: "IX_SportResourceAvailabilities_FkUserId",
                schema: "sports",
                table: "SportResourceAvailabilities",
                column: "FkUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SportResourceBlackouts_FkSportResourceId_StartUtc_EndUtc",
                schema: "sports",
                table: "SportResourceBlackouts",
                columns: new[] { "FkSportResourceId", "StartUtc", "EndUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_SportResourceBlackouts_FkUserId",
                schema: "sports",
                table: "SportResourceBlackouts",
                column: "FkUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SportResourceMedia_FkSportResourceId_SortOrder",
                schema: "sports",
                table: "SportResourceMedia",
                columns: new[] { "FkSportResourceId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_SportResourceMedia_FkStoredFileId",
                schema: "sports",
                table: "SportResourceMedia",
                column: "FkStoredFileId");

            migrationBuilder.CreateIndex(
                name: "IX_SportResourceMedia_FkUserId",
                schema: "sports",
                table: "SportResourceMedia",
                column: "FkUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SportResourcePricingRules_FkSportResourceId_DayOfWeek_Start~",
                schema: "sports",
                table: "SportResourcePricingRules",
                columns: new[] { "FkSportResourceId", "DayOfWeek", "StartTime", "EndTime" });

            migrationBuilder.CreateIndex(
                name: "IX_SportResourcePricingRules_FkUserId",
                schema: "sports",
                table: "SportResourcePricingRules",
                column: "FkUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SportResources_FkTenantId",
                schema: "sports",
                table: "SportResources",
                column: "FkTenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SportResources_FkTenantId_FkTenantSportTypeId",
                schema: "sports",
                table: "SportResources",
                columns: new[] { "FkTenantId", "FkTenantSportTypeId" });

            migrationBuilder.CreateIndex(
                name: "IX_SportResources_FkTenantSportTypeId",
                schema: "sports",
                table: "SportResources",
                column: "FkTenantSportTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SportResources_FkUserId",
                schema: "sports",
                table: "SportResources",
                column: "FkUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantBookingCategories_FkBookingCategoryId",
                schema: "booking",
                table: "TenantBookingCategories",
                column: "FkBookingCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantBookingCategories_FkTenantId",
                schema: "booking",
                table: "TenantBookingCategories",
                column: "FkTenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantBookingCategories_FkTenantId_FkBookingCategoryId",
                schema: "booking",
                table: "TenantBookingCategories",
                columns: new[] { "FkTenantId", "FkBookingCategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantBookingCategories_FkUserId",
                schema: "booking",
                table: "TenantBookingCategories",
                column: "FkUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantSportMedia_FkStoredFileId",
                schema: "sports",
                table: "TenantSportMedia",
                column: "FkStoredFileId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantSportMedia_FkTenantSportTypeId_SortOrder",
                schema: "sports",
                table: "TenantSportMedia",
                columns: new[] { "FkTenantSportTypeId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_TenantSportMedia_FkUserId",
                schema: "sports",
                table: "TenantSportMedia",
                column: "FkUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantSportTypes_FkTenantBookingCategoryId",
                schema: "sports",
                table: "TenantSportTypes",
                column: "FkTenantBookingCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantSportTypes_FkTenantId",
                schema: "sports",
                table: "TenantSportTypes",
                column: "FkTenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantSportTypes_FkTenantId_Name",
                schema: "sports",
                table: "TenantSportTypes",
                columns: new[] { "FkTenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantSportTypes_FkUserId",
                schema: "sports",
                table: "TenantSportTypes",
                column: "FkUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantWorkingHours_FkTenantId",
                schema: "tenants",
                table: "TenantWorkingHours",
                column: "FkTenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantWorkingHours_FkTenantId_DayOfWeek",
                schema: "tenants",
                table: "TenantWorkingHours",
                columns: new[] { "FkTenantId", "DayOfWeek" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TenantWorkingHours_FkUserId",
                schema: "tenants",
                table: "TenantWorkingHours",
                column: "FkUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookingApprovals",
                schema: "booking");

            migrationBuilder.DropTable(
                name: "SportResourceAvailabilities",
                schema: "sports");

            migrationBuilder.DropTable(
                name: "SportResourceBlackouts",
                schema: "sports");

            migrationBuilder.DropTable(
                name: "SportResourceMedia",
                schema: "sports");

            migrationBuilder.DropTable(
                name: "SportResourcePricingRules",
                schema: "sports");

            migrationBuilder.DropTable(
                name: "TenantSportMedia",
                schema: "sports");

            migrationBuilder.DropTable(
                name: "TenantWorkingHours",
                schema: "tenants");

            migrationBuilder.DropTable(
                name: "Bookings",
                schema: "booking");

            migrationBuilder.DropTable(
                name: "SportResources",
                schema: "sports");

            migrationBuilder.DropTable(
                name: "TenantSportTypes",
                schema: "sports");

            migrationBuilder.DropTable(
                name: "TenantBookingCategories",
                schema: "booking");

            migrationBuilder.DropTable(
                name: "BookingCategories",
                schema: "booking");
        }
    }
}
