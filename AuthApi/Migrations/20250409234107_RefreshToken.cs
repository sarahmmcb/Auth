using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthApi.Migrations
{
    /// <inheritdoc />
    public partial class RefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RefreshToken",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Expires = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Revoked = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshToken", x => x.Id);
                });

            migrationBuilder.UpdateData(
                schema: "core",
                table: "User",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$yIyAzG2qqVpGYr4vvW5Rhu0zAQBlo3GuxUsc/gyyvoqIOBC98A91W");

            migrationBuilder.UpdateData(
                schema: "core",
                table: "User",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$jlxgg3huqAkKy9vN6enwred6wUvH.9B6LVTZWCLBZbcCmgvnXaCri");

            migrationBuilder.CreateIndex(
                name: "IX_User_UserName",
                schema: "core",
                table: "User",
                column: "UserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefreshToken",
                schema: "core");

            migrationBuilder.DropIndex(
                name: "IX_User_UserName",
                schema: "core",
                table: "User");

            migrationBuilder.UpdateData(
                schema: "core",
                table: "User",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "BobsPassword");

            migrationBuilder.UpdateData(
                schema: "core",
                table: "User",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "HelensPassword");
        }
    }
}
