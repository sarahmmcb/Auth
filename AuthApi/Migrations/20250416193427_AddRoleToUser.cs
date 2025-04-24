using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthApi.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RoleId",
                schema: "core",
                table: "User",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                schema: "core",
                table: "Role",
                columns: new[] { "Id", "UserRole" },
                values: new object[] { 2, "User" });

            migrationBuilder.UpdateData(
                schema: "core",
                table: "User",
                keyColumn: "Id",
                keyValue: 1,
                column: "RoleId",
                value: 1);

            migrationBuilder.UpdateData(
                schema: "core",
                table: "User",
                keyColumn: "Id",
                keyValue: 2,
                column: "RoleId",
                value: 2);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "core",
                table: "Role",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DropColumn(
                name: "RoleId",
                schema: "core",
                table: "User");
        }
    }
}
