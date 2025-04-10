using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PrivateLMS.Models;

namespace PrivateLMS.Data
{
    public class LibraryDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Required for Identity tables

            // Seed Authors
            modelBuilder.Entity<Author>().HasData(
                new Author { AuthorId = 1, Name = "Muhammad Ibn Abdil-Wahhab" },
                new Author { AuthorId = 2, Name = "Imaam An-Nawawi" },
                new Author { AuthorId = 3, Name = "Imaam At-Tabari" },
                new Author { AuthorId = 4, Name = "Ibn Kathir" }
            );

            // Seed Books
            modelBuilder.Entity<Book>().HasData(
                new Book { BookId = 1, Title = "The Three Fundamental Principles", AuthorId = 1, ISBN = "978-0201616224", Language = "Arabic", PublishedDate = new DateTime(2021, 10, 30), Description = "A foundational text on Islamic principles.", AvailableCopies = 5, IsAvailable = true, PublisherId = 1 },
                new Book { BookId = 2, Title = "Arbaun An-Nawawi", AuthorId = 2, ISBN = "978-0132350884", Language = "Hausa", PublishedDate = new DateTime(2023, 8, 1), Description = "A collection of forty hadiths.", AvailableCopies = 3, IsAvailable = true, PublisherId = 2 },
                new Book { BookId = 3, Title = "Tafseer At-Tabari", AuthorId = 3, ISBN = "978-0451616235", Language = "Arabic", PublishedDate = new DateTime(2022, 11, 22), Description = "Comprehensive exegesis of the Quran.", AvailableCopies = 2, IsAvailable = true, PublisherId = 3 },
                new Book { BookId = 4, Title = "Tafseer Ibn Kathir", AuthorId = 4, ISBN = "978-4562350123", Language = "English", PublishedDate = new DateTime(2020, 8, 15), Description = "A widely respected Quranic commentary.", AvailableCopies = 4, IsAvailable = true, PublisherId = 4 }
            );

            // Seed Users with SecurityStamp
            modelBuilder.Entity<ApplicationUser>().HasData(
                new ApplicationUser
                {
                    Id = 1,
                    UserName = "admin",
                    NormalizedUserName = "ADMIN",
                    Email = "admin@privatelms.com",
                    NormalizedEmail = "ADMIN@PRIVATELMS.COM",
                    EmailConfirmed = true,
                    PasswordHash = "AQAAAAEAACcQAAAAEHs5oK5x5nL5z5bKX5n5z5bKX5n5z5bKX5n5z5bKX5n5z5bKX5n5z5bKX5n5z5bKXw==", // "password" hashed
                    SecurityStamp = "b7e8c9d0-1f2e-4a3b-8c9d-0e1f2e4a3b8c", // Unique GUID
                    PhoneNumber = "1234567890",
                    FirstName = "Admin",
                    LastName = "User",
                    Gender = "Male",
                    DateOfBirth = new DateTime(1990, 1, 1),
                    Address = "123 Admin St.",
                    City = "Admin City",
                    State = "Admin State",
                    PostalCode = "12345",
                    Country = "Admin Country",
                    TermsAccepted = true
                },
                new ApplicationUser
                {
                    Id = 2,
                    UserName = "user1",
                    NormalizedUserName = "USER1",
                    Email = "john.doe@example.com",
                    NormalizedEmail = "JOHN.DOE@EXAMPLE.COM",
                    EmailConfirmed = true,
                    PasswordHash = "AQAAAAEAACcQAAAAEHs5oK5x5nL5z5bKX5n5z5bKX5n5z5bKX5n5z5bKX5n5z5bKX5n5z5bKX5n5z5bKXw==", // "userpass" hashed
                    SecurityStamp = "d4f6a7b9-2c3e-4d5f-9a7b-92c3e4d5f9a7", // Unique GUID
                    PhoneNumber = "0987654321",
                    FirstName = "John",
                    LastName = "Doe",
                    Gender = "Male",
                    DateOfBirth = new DateTime(1995, 5, 15),
                    Address = "456 User Rd.",
                    City = "User City",
                    State = "User State",
                    PostalCode = "54321",
                    Country = "User Country",
                    TermsAccepted = true
                },
                new ApplicationUser
                {
                    Id = 3,
                    UserName = "admin2",
                    NormalizedUserName = "ADMIN2",
                    Email = "admin2@privatelms.com",
                    NormalizedEmail = "ADMIN2@PRIVATELMS.COM",
                    EmailConfirmed = true,
                    PasswordHash = "AQAAAAIAAYagAAAAEHk5z5bKX5n5z5bKX5n5z5bKX5n5z5bKX5n5z5bKX5n5z5bKX5n5z5bKX5n5z5bKXw==", // "Admin@456" hashed (placeholder)
                    SecurityStamp = "e8c9d0f1-3b4a-5c6d-9e0f-13b4a5c6d9e0", // Unique GUID
                    PhoneNumber = "0987654321",
                    FirstName = "Admin",
                    LastName = "Two",
                    Gender = "Male",
                    DateOfBirth = new DateTime(1985, 5, 15),
                    Address = "456 Admin Ave",
                    City = "Admin City",
                    State = "Admin State",
                    PostalCode = "54321",
                    Country = "Admin Country",
                    TermsAccepted = true
                }
            );

            // Seed Roles
            modelBuilder.Entity<IdentityRole<int>>().HasData(
                new IdentityRole<int> { Id = 1, Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole<int> { Id = 2, Name = "User", NormalizedName = "USER" }
            );

            // Seed User Roles
            modelBuilder.Entity<IdentityUserRole<int>>().HasData(
                new IdentityUserRole<int> { UserId = 1, RoleId = 1 }, // admin -> Admin
                new IdentityUserRole<int> { UserId = 2, RoleId = 2 }, // user1 -> User
                new IdentityUserRole<int> { UserId = 3, RoleId = 1 }  // admin2 -> Admin
            );

            // Seed Publishers
            modelBuilder.Entity<Publisher>().HasData(
                new Publisher { PublisherId = 1, PublisherName = "Dar Al-Ifta", Location = "Riyadh" },
                new Publisher { PublisherId = 2, PublisherName = "Islamic Heritage Press", Location = "Kano" },
                new Publisher { PublisherId = 3, PublisherName = "Tabari Publications", Location = "Cairo" },
                new Publisher { PublisherId = 4, PublisherName = "Kathir Books", Location = "London" }
            );

            // Seed Categories
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, CategoryName = "Islamic Principles" },
                new Category { CategoryId = 2, CategoryName = "Hadith" },
                new Category { CategoryId = 3, CategoryName = "Tafsir" }
            );

            // Seed BookCategory
            modelBuilder.Entity<BookCategory>().HasData(
                new BookCategory { BookId = 1, CategoryId = 1 },
                new BookCategory { BookId = 2, CategoryId = 2 },
                new BookCategory { BookId = 3, CategoryId = 3 },
                new BookCategory { BookId = 4, CategoryId = 3 }
            );

            // Seed LoanRecords
            modelBuilder.Entity<LoanRecord>().HasData(
                new LoanRecord
                {
                    LoanRecordId = 1,
                    BookId = 1,
                    UserId = 2,
                    LoanDate = new DateTime(2025, 3, 15),
                    DueDate = new DateTime(2025, 4, 5),
                    ReturnDate = new DateTime(2025, 4, 7),
                    FineAmount = 3000m,
                    IsFinePaid = false,
                    IsRenewed = false
                },
                new LoanRecord
                {
                    LoanRecordId = 2,
                    BookId = 2,
                    UserId = 2,
                    LoanDate = new DateTime(2025, 3, 30),
                    DueDate = new DateTime(2025, 4, 20),
                    ReturnDate = null,
                    FineAmount = 0m,
                    IsFinePaid = false,
                    IsRenewed = false
                }
            );

            // Relationships
            modelBuilder.Entity<BookCategory>()
                .HasKey(bc => new { bc.BookId, bc.CategoryId });

            modelBuilder.Entity<BookCategory>()
                .HasOne(bc => bc.Book)
                .WithMany(b => b.BookCategories)
                .HasForeignKey(bc => bc.BookId);

            modelBuilder.Entity<BookCategory>()
                .HasOne(bc => bc.Category)
                .WithMany(c => c.BookCategories)
                .HasForeignKey(bc => bc.CategoryId);

            modelBuilder.Entity<Book>()
                .HasOne(b => b.Publisher)
                .WithMany()
                .HasForeignKey(b => b.PublisherId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Book>()
                .HasOne(b => b.Author)
                .WithMany(a => a.Books)
                .HasForeignKey(b => b.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LoanRecord>()
                .HasOne(lr => lr.Book)
                .WithMany()
                .HasForeignKey(lr => lr.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LoanRecord>()
                .HasOne(lr => lr.User)
                .WithMany()
                .HasForeignKey(lr => lr.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Index for performance
            modelBuilder.Entity<LoanRecord>()
                .HasIndex(lr => lr.UserId);
        }

        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<LoanRecord> LoanRecords { get; set; }
        public DbSet<Publisher> Publishers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<BookCategory> BookCategories { get; set; }
    }
}