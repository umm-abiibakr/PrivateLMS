using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrivateLMS.Migrations
{
    /// <inheritdoc />
    public partial class RemoveHasRequested : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasRequestedPayment",
                table: "Fines");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "e52564ef-bd60-443b-8458-4ef06d66c54b");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "fad12924-b2da-46a9-8809-456d90c8d670");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "ede8971f-a84a-4ed7-97be-7ba303bdd608");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasRequestedPayment",
                table: "Fines",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "97306553-5710-4870-9736-a0e42b9a7700");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "c87f2f5d-d09f-4f80-96ea-89e70ff99334");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "5e814132-ce8c-4c9d-848b-eefc4eaed695");

            migrationBuilder.UpdateData(
                table: "Fines",
                keyColumn: "Id",
                keyValue: 1,
                column: "HasRequestedPayment",
                value: false);
        }
    }
}
