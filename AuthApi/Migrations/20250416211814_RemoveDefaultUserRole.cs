using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDefaultUserRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "core",
                table: "UserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { 2, 2 });

            migrationBuilder.DeleteData(
                schema: "core",
                table: "Role",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.RenameColumn(
                name: "UserRole",
                schema: "core",
                table: "Role",
                newName: "RoleName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RoleName",
                schema: "core",
                table: "Role",
                newName: "UserRole");

            migrationBuilder.InsertData(
                schema: "core",
                table: "Role",
                columns: new[] { "Id", "UserRole" },
                values: new object[] { 2, "User" });

            migrationBuilder.InsertData(
                schema: "core",
                table: "UserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { 2, 2 });
        }
    }
}
