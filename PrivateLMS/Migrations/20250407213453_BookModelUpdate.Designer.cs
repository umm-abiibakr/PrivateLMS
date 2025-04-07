﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PrivateLMS.Data;

#nullable disable

namespace PrivateLMS.Migrations
{
    [DbContext(typeof(LibraryDbContext))]
    [Migration("20250407213453_BookModelUpdate")]
    partial class BookModelUpdate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("PrivateLMS.Models.Book", b =>
                {
                    b.Property<int>("BookId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("BookId"));

                    b.Property<string>("Author")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<int>("AvailableCopies")
                        .HasColumnType("int");

                    b.Property<string>("CoverImagePath")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ISBN")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsAvailable")
                        .HasColumnType("bit");

                    b.Property<string>("Language")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<DateTime>("PublishedDate")
                        .HasColumnType("datetime2");

                    b.Property<int?>("PublisherId")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("BookId");

                    b.HasIndex("PublisherId");

                    b.ToTable("Books");

                    b.HasData(
                        new
                        {
                            BookId = 1,
                            Author = "Muhammad Ibn Abdil-Wahhab",
                            AvailableCopies = 5,
                            Description = "A foundational text on Islamic principles.",
                            ISBN = "978-0201616224",
                            IsAvailable = true,
                            Language = "Arabic",
                            PublishedDate = new DateTime(2021, 10, 30, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            PublisherId = 1,
                            Title = "The Three Fundamental Principles"
                        },
                        new
                        {
                            BookId = 2,
                            Author = "Imaam An-Nawawi",
                            AvailableCopies = 3,
                            Description = "A collection of forty hadiths.",
                            ISBN = "978-0132350884",
                            IsAvailable = true,
                            Language = "Hausa",
                            PublishedDate = new DateTime(2023, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            PublisherId = 2,
                            Title = "Arbaun An-Nawawi"
                        },
                        new
                        {
                            BookId = 3,
                            Author = "Imaam At-Tabari",
                            AvailableCopies = 2,
                            Description = "Comprehensive exegesis of the Quran.",
                            ISBN = "978-0451616235",
                            IsAvailable = true,
                            Language = "Arabic",
                            PublishedDate = new DateTime(2022, 11, 22, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            PublisherId = 3,
                            Title = "Tafseer At-Tabari"
                        },
                        new
                        {
                            BookId = 4,
                            Author = "Ibn Kathir",
                            AvailableCopies = 4,
                            Description = "A widely respected Quranic commentary.",
                            ISBN = "978-4562350123",
                            IsAvailable = true,
                            Language = "English",
                            PublishedDate = new DateTime(2020, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            PublisherId = 4,
                            Title = "Tafseer Ibn Kathir"
                        });
                });

            modelBuilder.Entity("PrivateLMS.Models.BookCategory", b =>
                {
                    b.Property<int>("BookId")
                        .HasColumnType("int");

                    b.Property<int>("CategoryId")
                        .HasColumnType("int");

                    b.HasKey("BookId", "CategoryId");

                    b.HasIndex("CategoryId");

                    b.ToTable("BookCategory");

                    b.HasData(
                        new
                        {
                            BookId = 1,
                            CategoryId = 1
                        },
                        new
                        {
                            BookId = 2,
                            CategoryId = 2
                        },
                        new
                        {
                            BookId = 3,
                            CategoryId = 3
                        },
                        new
                        {
                            BookId = 4,
                            CategoryId = 3
                        });
                });

            modelBuilder.Entity("PrivateLMS.Models.Category", b =>
                {
                    b.Property<int>("CategoryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CategoryId"));

                    b.Property<string>("CategoryName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("CategoryId");

                    b.ToTable("Categories");

                    b.HasData(
                        new
                        {
                            CategoryId = 1,
                            CategoryName = "Islamic Principles"
                        },
                        new
                        {
                            CategoryId = 2,
                            CategoryName = "Hadith"
                        },
                        new
                        {
                            CategoryId = 3,
                            CategoryName = "Tafsir"
                        });
                });

            modelBuilder.Entity("PrivateLMS.Models.LoanRecord", b =>
                {
                    b.Property<int>("LoanRecordId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("LoanRecordId"));

                    b.Property<int>("BookId")
                        .HasColumnType("int");

                    b.Property<DateTime>("LoanDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("LoanerEmail")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LoanerName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ReturnDate")
                        .HasColumnType("datetime2");

                    b.HasKey("LoanRecordId");

                    b.HasIndex("BookId");

                    b.ToTable("LoanRecords");
                });

            modelBuilder.Entity("PrivateLMS.Models.Publisher", b =>
                {
                    b.Property<int>("PublisherId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("PublisherId"));

                    b.Property<string>("Location")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LogoImagePath")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PublisherName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("PublisherId");

                    b.ToTable("Publishers");

                    b.HasData(
                        new
                        {
                            PublisherId = 1,
                            Location = "Riyadh",
                            PublisherName = "Dar Al-Ifta"
                        },
                        new
                        {
                            PublisherId = 2,
                            Location = "Kano",
                            PublisherName = "Islamic Heritage Press"
                        },
                        new
                        {
                            PublisherId = 3,
                            Location = "Cairo",
                            PublisherName = "Tabari Publications"
                        },
                        new
                        {
                            PublisherId = 4,
                            Location = "London",
                            PublisherName = "Kathir Books"
                        });
                });

            modelBuilder.Entity("PrivateLMS.Models.User", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("UserId"));

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("City")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ConfirmPassword")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Country")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("DateOfBirth")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Gender")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PostalCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("State")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TermsAccepted")
                        .HasColumnType("bit");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("UserId");

                    b.ToTable("Users");

                    b.HasData(
                        new
                        {
                            UserId = 1,
                            Address = "123 Admin St.",
                            City = "Admin City",
                            ConfirmPassword = "password",
                            Country = "Admin Country",
                            DateOfBirth = new DateTime(1990, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Email = "admin@privatelms.com",
                            FirstName = "Admin",
                            Gender = "Male",
                            LastName = "User",
                            Password = "password",
                            PhoneNumber = "1234567890",
                            PostalCode = "12345",
                            Role = "Admin",
                            State = "Admin State",
                            TermsAccepted = true,
                            Username = "admin"
                        });
                });

            modelBuilder.Entity("PrivateLMS.Models.Book", b =>
                {
                    b.HasOne("PrivateLMS.Models.Publisher", "Publisher")
                        .WithMany()
                        .HasForeignKey("PublisherId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Publisher");
                });

            modelBuilder.Entity("PrivateLMS.Models.BookCategory", b =>
                {
                    b.HasOne("PrivateLMS.Models.Book", "Book")
                        .WithMany("BookCategories")
                        .HasForeignKey("BookId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PrivateLMS.Models.Category", "Category")
                        .WithMany("BookCategories")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Book");

                    b.Navigation("Category");
                });

            modelBuilder.Entity("PrivateLMS.Models.LoanRecord", b =>
                {
                    b.HasOne("PrivateLMS.Models.Book", "Book")
                        .WithMany("LoanRecords")
                        .HasForeignKey("BookId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Book");
                });

            modelBuilder.Entity("PrivateLMS.Models.Book", b =>
                {
                    b.Navigation("BookCategories");

                    b.Navigation("LoanRecords");
                });

            modelBuilder.Entity("PrivateLMS.Models.Category", b =>
                {
                    b.Navigation("BookCategories");
                });
#pragma warning restore 612, 618
        }
    }
}
