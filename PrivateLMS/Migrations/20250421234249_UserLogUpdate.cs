using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrivateLMS.Migrations
{
    /// <inheritdoc />
    public partial class UserLogUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EntityId",
                table: "UserActivities",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EntityType",
                table: "UserActivities",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IPAddress",
                table: "UserActivities",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                table: "UserActivities",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "UserActivities",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "3875a368-0b63-42aa-b389-347861741ce2");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "d76b9774-35c0-49ec-9c0d-5a00e464d66f");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "16a8e8bd-930c-465c-a37a-b00bc49a240b");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "UserActivities");

            migrationBuilder.DropColumn(
                name: "EntityType",
                table: "UserActivities");

            migrationBuilder.DropColumn(
                name: "IPAddress",
                table: "UserActivities");

            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "UserActivities");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "UserActivities");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "14ffe848-93a5-4383-aa7d-328dfe06d6c1");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "05a030cc-55d7-4aa8-ae6c-c8c976ab0945");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "f1a29377-76a9-4ce9-ba28-4fa352516d61");
        }
    }
}
