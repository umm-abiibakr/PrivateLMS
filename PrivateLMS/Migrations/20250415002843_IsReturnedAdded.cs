using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrivateLMS.Migrations
{
    /// <inheritdoc />
    public partial class IsReturnedAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsReturned",
                table: "LoanRecords");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "0f8190aa-5a64-4176-a3c1-516416619774");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "4b1900b8-f09b-48f9-8b0b-b2b5c471974c");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "2e62624e-fdd8-4465-acbc-78b60eca57a5");
        }
    }
}
