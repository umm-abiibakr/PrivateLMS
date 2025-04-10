﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PrivateLMS.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Authors",
                columns: table => new
                {
                    AuthorId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Biography = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeathDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Authors", x => x.AuthorId);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "Publishers",
                columns: table => new
                {
                    PublisherId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PublisherName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LogoImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Publishers", x => x.PublisherId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ConfirmPassword = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TermsAccepted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    BookId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Language = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AuthorId = table.Column<int>(type: "int", nullable: false),
                    ISBN = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PublishedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AvailableCopies = table.Column<int>(type: "int", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    CoverImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PublisherId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.BookId);
                    table.ForeignKey(
                        name: "FK_Books_Authors_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Authors",
                        principalColumn: "AuthorId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Books_Publishers_PublisherId",
                        column: x => x.PublisherId,
                        principalTable: "Publishers",
                        principalColumn: "PublisherId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "BookCategories",
                columns: table => new
                {
                    BookId = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BookCategories", x => new { x.BookId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_BookCategories_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "BookId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BookCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LoanRecords",
                columns: table => new
                {
                    LoanRecordId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    LoanDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReturnDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FineAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsFinePaid = table.Column<bool>(type: "bit", nullable: false),
                    IsRenewed = table.Column<bool>(type: "bit", nullable: false),
                    BookId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanRecords", x => x.LoanRecordId);
                    table.ForeignKey(
                        name: "FK_LoanRecords_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "BookId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LoanRecords_Books_BookId1",
                        column: x => x.BookId1,
                        principalTable: "Books",
                        principalColumn: "BookId");
                    table.ForeignKey(
                        name: "FK_LoanRecords_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Authors",
                columns: new[] { "AuthorId", "Biography", "BirthDate", "DeathDate", "Name" },
                values: new object[,]
                {
                    { 1, null, null, null, "Muhammad Ibn Abdil-Wahhab" },
                    { 2, null, null, null, "Imaam An-Nawawi" },
                    { 3, null, null, null, "Imaam At-Tabari" },
                    { 4, null, null, null, "Ibn Kathir" }
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "CategoryId", "CategoryName" },
                values: new object[,]
                {
                    { 1, "Islamic Principles" },
                    { 2, "Hadith" },
                    { 3, "Tafsir" }
                });

            migrationBuilder.InsertData(
                table: "Publishers",
                columns: new[] { "PublisherId", "Location", "LogoImagePath", "PublisherName" },
                values: new object[,]
                {
                    { 1, "Riyadh", null, "Dar Al-Ifta" },
                    { 2, "Kano", null, "Islamic Heritage Press" },
                    { 3, "Cairo", null, "Tabari Publications" },
                    { 4, "London", null, "Kathir Books" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "Address", "City", "ConfirmPassword", "Country", "DateOfBirth", "Email", "FirstName", "Gender", "LastName", "Password", "PhoneNumber", "PostalCode", "Role", "State", "TermsAccepted", "Username" },
                values: new object[,]
                {
                    { 1, "123 Admin St.", "Admin City", "password", "Admin Country", new DateTime(1990, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@privatelms.com", "Admin", "Male", "User", "password", "1234567890", "12345", "Admin", "Admin State", true, "admin" },
                    { 2, "456 User Rd.", "User City", "userpass", "User Country", new DateTime(1995, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "john.doe@example.com", "John", "Male", "Doe", "userpass", "0987654321", "54321", "User", "User State", true, "user1" }
                });

            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "BookId", "AuthorId", "AvailableCopies", "CoverImagePath", "Description", "ISBN", "IsAvailable", "Language", "PublishedDate", "PublisherId", "Title" },
                values: new object[,]
                {
                    { 1, 1, 5, null, "A foundational text on Islamic principles.", "978-0201616224", true, "Arabic", new DateTime(2021, 10, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "The Three Fundamental Principles" },
                    { 2, 2, 3, null, "A collection of forty hadiths.", "978-0132350884", true, "Hausa", new DateTime(2023, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, "Arbaun An-Nawawi" },
                    { 3, 3, 2, null, "Comprehensive exegesis of the Quran.", "978-0451616235", true, "Arabic", new DateTime(2022, 11, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, "Tafseer At-Tabari" },
                    { 4, 4, 4, null, "A widely respected Quranic commentary.", "978-4562350123", true, "English", new DateTime(2020, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, "Tafseer Ibn Kathir" }
                });

            migrationBuilder.InsertData(
                table: "BookCategories",
                columns: new[] { "BookId", "CategoryId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 2 },
                    { 3, 3 },
                    { 4, 3 }
                });

            migrationBuilder.InsertData(
                table: "LoanRecords",
                columns: new[] { "LoanRecordId", "BookId", "BookId1", "DueDate", "FineAmount", "IsFinePaid", "IsRenewed", "LoanDate", "ReturnDate", "UserId" },
                values: new object[,]
                {
                    { 1, 1, null, new DateTime(2025, 4, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), 3000m, false, false, new DateTime(2025, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 4, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), 2 },
                    { 2, 2, null, new DateTime(2025, 4, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), 0m, false, false, new DateTime(2025, 3, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), null, 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BookCategories_CategoryId",
                table: "BookCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Books_AuthorId",
                table: "Books",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_Books_PublisherId",
                table: "Books",
                column: "PublisherId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanRecords_BookId",
                table: "LoanRecords",
                column: "BookId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanRecords_BookId1",
                table: "LoanRecords",
                column: "BookId1");

            migrationBuilder.CreateIndex(
                name: "IX_LoanRecords_UserId",
                table: "LoanRecords",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BookCategories");

            migrationBuilder.DropTable(
                name: "LoanRecords");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Books");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Authors");

            migrationBuilder.DropTable(
                name: "Publishers");
        }
    }
}
