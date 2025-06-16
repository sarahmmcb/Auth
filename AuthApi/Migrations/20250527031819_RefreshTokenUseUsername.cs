using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthApi.Migrations
{
    /// <inheritdoc />
    public partial class RefreshTokenUseUsername : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "core",
                table: "RefreshToken");

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                schema: "core",
                table: "RefreshToken",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_UserName",
                schema: "core",
                table: "RefreshToken",
                column: "UserName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RefreshToken_UserName",
                schema: "core",
                table: "RefreshToken");

            migrationBuilder.DropColumn(
                name: "UserName",
                schema: "core",
                table: "RefreshToken");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                schema: "core",
                table: "RefreshToken",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
