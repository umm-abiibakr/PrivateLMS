using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrivateLMS.Migrations
{
    /// <inheritdoc />
    public partial class AddSecurityStampToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "211196cd-1113-44ab-8a47-d64f963e763c", "b7e8c9d0-1f2e-4a3b-8c9d-0e1f2e4a3b8c" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "6a7f2c68-6391-4ae3-a0d5-13a6db666b69", "d4f6a7b9-2c3e-4d5f-9a7b-92c3e4d5f9a7" });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "Address", "City", "ConcurrencyStamp", "Country", "DateOfBirth", "Email", "EmailConfirmed", "FirstName", "Gender", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "PostalCode", "SecurityStamp", "State", "TermsAccepted", "TwoFactorEnabled", "UserName" },
                values: new object[] { 3, 0, "456 Admin Ave", "Admin City", "7a24418f-efb8-4644-8cdf-7e425f060266", "Admin Country", new DateTime(1985, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin2@privatelms.com", true, "Admin", "Male", "Two", false, null, "ADMIN2@PRIVATELMS.COM", "ADMIN2", "AQAAAAIAAYagAAAAEHk5z5bKX5n5z5bKX5n5z5bKX5n5z5bKX5n5z5bKX5n5z5bKX5n5z5bKX5n5z5bKXw==", "0987654321", false, "54321", "e8c9d0f1-3b4a-5c6d-9e0f-13b4a5c6d9e0", "Admin State", true, false, "admin2" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { 1, 3 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { 1, 3 });

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "f6b5acf7-7042-4809-ab9d-17c5dd809e13", null });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "12761294-1552-47ca-ad64-a7fabc099235", null });
        }
    }
}
