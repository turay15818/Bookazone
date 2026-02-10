using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookazone.Migrations
{
    /// <inheritdoc />
    public partial class _20260208190000_AddEventOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventOrders",
                schema: "events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FkEventId = table.Column<Guid>(type: "uuid", nullable: false),
                    FkTenantId = table.Column<Guid>(type: "uuid", nullable: false),
                    FkCustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PaymentStatus = table.Column<int>(type: "integer", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    TotalQuantity = table.Column<int>(type: "integer", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ContactName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ContactEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ContactPhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
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
                    table.PrimaryKey("PK_EventOrders", x => x.Id);
                    table.CheckConstraint("CK_EventOrders_TotalQuantity", "\"TotalQuantity\" > 0");
                    table.ForeignKey(
                        name: "FK_EventOrders_Events_FkEventId",
                        column: x => x.FkEventId,
                        principalSchema: "events",
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventOrders_Tenants_FkTenantId",
                        column: x => x.FkTenantId,
                        principalSchema: "tenants",
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventOrders_Users_FkCustomerId",
                        column: x => x.FkCustomerId,
                        principalSchema: "profile",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventOrders_Users_FkUserId",
                        column: x => x.FkUserId,
                        principalSchema: "profile",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EventOrderItems",
                schema: "events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FkEventOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    FkEventTicketTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    TicketName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    Subtotal = table.Column<decimal>(type: "numeric", nullable: false),
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
                    table.PrimaryKey("PK_EventOrderItems", x => x.Id);
                    table.CheckConstraint("CK_EventOrderItems_Quantity", "\"Quantity\" > 0");
                    table.ForeignKey(
                        name: "FK_EventOrderItems_EventOrders_FkEventOrderId",
                        column: x => x.FkEventOrderId,
                        principalSchema: "events",
                        principalTable: "EventOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventOrderItems_EventTicketTypes_FkEventTicketTypeId",
                        column: x => x.FkEventTicketTypeId,
                        principalSchema: "events",
                        principalTable: "EventTicketTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventOrderItems_Users_FkUserId",
                        column: x => x.FkUserId,
                        principalSchema: "profile",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventOrderItems_FkEventOrderId",
                schema: "events",
                table: "EventOrderItems",
                column: "FkEventOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_EventOrderItems_FkEventTicketTypeId",
                schema: "events",
                table: "EventOrderItems",
                column: "FkEventTicketTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_EventOrderItems_FkUserId",
                schema: "events",
                table: "EventOrderItems",
                column: "FkUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EventOrders_ExpiresAtUtc",
                schema: "events",
                table: "EventOrders",
                column: "ExpiresAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_EventOrders_FkCustomerId",
                schema: "events",
                table: "EventOrders",
                column: "FkCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_EventOrders_FkEventId",
                schema: "events",
                table: "EventOrders",
                column: "FkEventId");

            migrationBuilder.CreateIndex(
                name: "IX_EventOrders_FkEventId_Status",
                schema: "events",
                table: "EventOrders",
                columns: new[] { "FkEventId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_EventOrders_FkTenantId",
                schema: "events",
                table: "EventOrders",
                column: "FkTenantId");

            migrationBuilder.CreateIndex(
                name: "IX_EventOrders_FkTenantId_Status",
                schema: "events",
                table: "EventOrders",
                columns: new[] { "FkTenantId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_EventOrders_FkUserId",
                schema: "events",
                table: "EventOrders",
                column: "FkUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventOrderItems",
                schema: "events");

            migrationBuilder.DropTable(
                name: "EventOrders",
                schema: "events");
        }
    }
}
