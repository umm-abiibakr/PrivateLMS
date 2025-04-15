using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrivateLMS.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIsReturned : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsReturned",
                table: "LoanRecords");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "0a2e4d56-7212-4e56-a237-c3351c173dd7");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "c2464dd3-0bbd-4e0f-83c6-78144768a0dc");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "a6552d5a-771f-4ea2-8e03-719122aff36c");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsReturned",
                table: "LoanRecords",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "04e6cd12-3136-4c72-9942-24c266b48744");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "df414425-e3de-4d9f-86fb-646c4907ad77");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "1d26e5d8-87d7-4c18-a2cd-a82a4574f3bb");

            migrationBuilder.UpdateData(
                table: "LoanRecords",
                keyColumn: "LoanRecordId",
                keyValue: 1,
                column: "IsReturned",
                value: false);

            migrationBuilder.UpdateData(
                table: "LoanRecords",
                keyColumn: "LoanRecordId",
                keyValue: 2,
                column: "IsReturned",
                value: false);
        }
    }
}
