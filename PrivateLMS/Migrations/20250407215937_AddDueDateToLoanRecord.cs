using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrivateLMS.Migrations
{
    /// <inheritdoc />
    public partial class AddDueDateToLoanRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "LoanRecords",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "LoanRecords");
        }
    }
}
