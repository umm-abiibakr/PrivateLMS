using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrivateLMS.Migrations
{
    /// <inheritdoc />
    public partial class PublisherFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Publishers_Books_BookId",
                table: "Publishers");

            migrationBuilder.DropForeignKey(
                name: "FK_Publishers_Books_BookId1",
                table: "Publishers");

            migrationBuilder.DropIndex(
                name: "IX_Publishers_BookId",
                table: "Publishers");

            migrationBuilder.DropIndex(
                name: "IX_Publishers_BookId1",
                table: "Publishers");

            migrationBuilder.DropColumn(
                name: "BookId",
                table: "Publishers");

            migrationBuilder.DropColumn(
                name: "BookId1",
                table: "Publishers");

            migrationBuilder.AddColumn<int>(
                name: "PublisherId",
                table: "Books",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "BookId",
                keyValue: 1,
                column: "PublisherId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "BookId",
                keyValue: 2,
                column: "PublisherId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "BookId",
                keyValue: 3,
                column: "PublisherId",
                value: 3);

            migrationBuilder.UpdateData(
                table: "Books",
                keyColumn: "BookId",
                keyValue: 4,
                column: "PublisherId",
                value: 4);

            migrationBuilder.CreateIndex(
                name: "IX_Books_PublisherId",
                table: "Books",
                column: "PublisherId");

            migrationBuilder.AddForeignKey(
                name: "FK_Books_Publishers_PublisherId",
                table: "Books",
                column: "PublisherId",
                principalTable: "Publishers",
                principalColumn: "PublisherId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Books_Publishers_PublisherId",
                table: "Books");

            migrationBuilder.DropIndex(
                name: "IX_Books_PublisherId",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "PublisherId",
                table: "Books");

            migrationBuilder.AddColumn<int>(
                name: "BookId",
                table: "Publishers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BookId1",
                table: "Publishers",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Publishers",
                keyColumn: "PublisherId",
                keyValue: 1,
                columns: new[] { "BookId", "BookId1" },
                values: new object[] { 1, null });

            migrationBuilder.UpdateData(
                table: "Publishers",
                keyColumn: "PublisherId",
                keyValue: 2,
                columns: new[] { "BookId", "BookId1" },
                values: new object[] { 2, null });

            migrationBuilder.UpdateData(
                table: "Publishers",
                keyColumn: "PublisherId",
                keyValue: 3,
                columns: new[] { "BookId", "BookId1" },
                values: new object[] { 3, null });

            migrationBuilder.UpdateData(
                table: "Publishers",
                keyColumn: "PublisherId",
                keyValue: 4,
                columns: new[] { "BookId", "BookId1" },
                values: new object[] { 4, null });

            migrationBuilder.CreateIndex(
                name: "IX_Publishers_BookId",
                table: "Publishers",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_Publishers_BookId1",
                table: "Publishers",
                column: "BookId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Publishers_Books_BookId",
                table: "Publishers",
                column: "BookId",
                principalTable: "Books",
                principalColumn: "BookId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Publishers_Books_BookId1",
                table: "Publishers",
                column: "BookId1",
                principalTable: "Books",
                principalColumn: "BookId");
        }
    }
}
