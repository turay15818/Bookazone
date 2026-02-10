using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookazone.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EmailVerified",
                schema: "profile",
                table: "AuthProviders",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PictureUrl",
                schema: "profile",
                table: "AuthProviders",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuthProviders_Provider_ProviderUserId",
                schema: "profile",
                table: "AuthProviders",
                columns: new[] { "Provider", "ProviderUserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuthProviders_Provider_ProviderUserId",
                schema: "profile",
                table: "AuthProviders");

            migrationBuilder.DropColumn(
                name: "EmailVerified",
                schema: "profile",
                table: "AuthProviders");

            migrationBuilder.DropColumn(
                name: "PictureUrl",
                schema: "profile",
                table: "AuthProviders");
        }
    }
}
