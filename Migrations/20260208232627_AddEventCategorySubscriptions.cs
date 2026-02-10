using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookazone.Migrations
{
    /// <inheritdoc />
    public partial class AddEventCategorySubscriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventCategorySubscriptions",
                schema: "events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FkEventCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceivePush = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    ReceiveEmail = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_EventCategorySubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventCategorySubscriptions_EventCategories_FkEventCategoryId",
                        column: x => x.FkEventCategoryId,
                        principalSchema: "events",
                        principalTable: "EventCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventCategorySubscriptions_Users_FkUserId",
                        column: x => x.FkUserId,
                        principalSchema: "profile",
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventCategorySubscriptions_FkEventCategoryId",
                schema: "events",
                table: "EventCategorySubscriptions",
                column: "FkEventCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_EventCategorySubscriptions_FkUserId",
                schema: "events",
                table: "EventCategorySubscriptions",
                column: "FkUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EventCategorySubscriptions_FkUserId_FkEventCategoryId",
                schema: "events",
                table: "EventCategorySubscriptions",
                columns: new[] { "FkUserId", "FkEventCategoryId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventCategorySubscriptions",
                schema: "events");
        }
    }
}
