using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PrivateLMS.Models;

namespace PrivateLMS.Data
{
    public class LibraryDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options)
            : base(options) { }

        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<LoanRecord> LoanRecords { get; set; }
        public DbSet<Publisher> Publishers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<BookCategory> BookCategories { get; set; }
        public DbSet<UserActivity> UserActivities { get; set; }
        public DbSet<BookRating> BookRatings { get; set; } = null!;
        public DbSet<Fine> Fines { get; set; } = null!;
        public DbSet<Language> Languages { get; set; }
        public DbSet<UserPreference> UserPreferences { get; set; }
        public DbSet<CategoryPreference> CategoryPreferences { get; set; }
        public DbSet<AuthorPreference> AuthorPreferences { get; set; }
        public DbSet<LanguagePreference> LanguagePreferences { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Identity configuration

            // Seed Authors
            modelBuilder.Entity<Author>().HasData(
                new Author { AuthorId = 1, Name = "Muhammad Ibn Abdil-Wahhab" },
                new Author { AuthorId = 2, Name = "Imaam An-Nawawi" },
                new Author { AuthorId = 3, Name = "Imaam At-Tabari" },
                new Author { AuthorId = 4, Name = "Ibn Kathir" }
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

            // Seed Languages
            modelBuilder.Entity<Language>().HasData(
                new Language { LanguageId = 1, Name = "Arabic" },
                new Language { LanguageId = 2, Name = "English" },
                new Language { LanguageId = 3, Name = "Hausa" }
            );



            // Seed Books
            modelBuilder.Entity<Book>().HasData(
                new Book
                {
                    BookId = 1,
                    Title = "The Three Fundamental Principles",
                    AuthorId = 1,
                    ISBN = "978-0201616224",
                    LanguageId = 1,
                    PublishedDate = new DateTime(2021, 10, 30),
                    Description = "A foundational text on Islamic principles.",
                    AvailableCopies = 5,
                    IsAvailable = true,
                    PublisherId = 1
                },
                new Book
                {
                    BookId = 2,
                    Title = "Arbaun An-Nawawi",
                    AuthorId = 2,
                    ISBN = "978-0132350884",
                    LanguageId = 2,
                    PublishedDate = new DateTime(2023, 8, 1),
                    Description = "A collection of forty hadiths.",
                    AvailableCopies = 3,
                    IsAvailable = true,
                    PublisherId = 2
                },
                new Book
                {
                    BookId = 3,
                    Title = "Tafseer At-Tabari",
                    AuthorId = 3,
                    ISBN = "978-0451616235",
                    LanguageId = 3,
                    PublishedDate = new DateTime(2022, 11, 22),
                    Description = "Comprehensive exegesis of the Quran.",
                    AvailableCopies = 2,
                    IsAvailable = true,
                    PublisherId = 3
                },
                new Book
                {
                    BookId = 4,
                    Title = "Tafseer Ibn Kathir",
                    AuthorId = 4,
                    ISBN = "978-4562350123",
                    LanguageId = 3,
                    PublishedDate = new DateTime(2020, 8, 15),
                    Description = "A widely respected Quranic commentary.",
                    AvailableCopies = 4,
                    IsAvailable = true,
                    PublisherId = 4
                }
            );

            // Seed BookCategory relations
            modelBuilder.Entity<BookCategory>().HasData(
                new BookCategory { BookId = 1, CategoryId = 1 },
                new BookCategory { BookId = 2, CategoryId = 2 },
                new BookCategory { BookId = 3, CategoryId = 3 },
                new BookCategory { BookId = 4, CategoryId = 3 }
            );

            // Seed Users
            modelBuilder.Entity<ApplicationUser>().HasData(
                new ApplicationUser
                {
                    Id = 1,
                    UserName = "admin",
                    NormalizedUserName = "ADMIN",
                    Email = "admin@privatelms.com",
                    NormalizedEmail = "ADMIN@PRIVATELMS.COM",
                    EmailConfirmed = true,
                    PasswordHash = "AQAAAAEAACcQAAAAEHs5oK5x5nL5z5bKX5n5z5bKX5n5z5bKX5n5z5bKX5n5z5bKX5n5z5bKX5n5z5bKXw==",
                    SecurityStamp = "b7e8c9d0-1f2e-4a3b-8c9d-0e1f2e4a3b8c",
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
                    TermsAccepted = true,
                    IsApproved = true
                },
                new ApplicationUser
                {
                    Id = 2,
                    UserName = "user1",
                    NormalizedUserName = "USER1",
                    Email = "john.doe@example.com",
                    NormalizedEmail = "JOHN.DOE@EXAMPLE.COM",
                    EmailConfirmed = true,
                    PasswordHash = "AQAAAAEAACcQAAAAEHs5oK5x5nL5z5bKX5n5z5bKX5n5z5bKX5n5z5bKX5n5z5bKX5n5z5bKX5n5z5bKXw==",
                    SecurityStamp = "d4f6a7b9-2c3e-4d5f-9a7b-92c3e4d5f9a7",
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
                    TermsAccepted = true,
                    IsApproved = true
                },
                new ApplicationUser
                {
                    Id = 3,
                    UserName = "admin2",
                    NormalizedUserName = "ADMIN2",
                    Email = "admin2@privatelms.com",
                    NormalizedEmail = "ADMIN2@PRIVATELMS.COM",
                    EmailConfirmed = true,
                    PasswordHash = "AQAAAAIAAYagAAAAEHk5z5bKX5n5z5bKX5n5z5bKX5n5z5bKX5n5z5bKX5n5z5bKX5n5z5bKX5n5z5bKXw==",
                    SecurityStamp = "e8c9d0f1-3b4a-5c6d-9e0f-13b4a5c6d9e0",
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
                    TermsAccepted = true,
                    IsApproved = true
                }
            );

            // Seed Roles
            modelBuilder.Entity<IdentityRole<int>>().HasData(
                new IdentityRole<int> { Id = 1, Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole<int> { Id = 2, Name = "User", NormalizedName = "USER" }
            );

            // Seed User Roles
            modelBuilder.Entity<IdentityUserRole<int>>().HasData(
                new IdentityUserRole<int> { UserId = 1, RoleId = 1 },
                new IdentityUserRole<int> { UserId = 2, RoleId = 2 },
                new IdentityUserRole<int> { UserId = 3, RoleId = 1 }
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
                    IsRenewed = false
                }
            );

            // Seed Fines
            modelBuilder.Entity<Fine>().HasData(
                new Fine
                {
                    Id = 1,
                    UserId = 2,
                    LoanId = 1,
                    Amount = 3000m,
                    IssuedDate = new DateTime(2025, 4, 7),
                    IsPaid = false
                }
            );

            // Relationships
            modelBuilder.Entity<BookCategory>().HasKey(bc => new { bc.BookId, bc.CategoryId });

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
                .HasOne(lr => lr.User)
                .WithMany()
                .HasForeignKey(lr => lr.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LoanRecord>()
                .HasOne(lr => lr.Book)
                .WithMany()
                .HasForeignKey(lr => lr.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Fine>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Fine>()
              .HasOne(f => f.LoanRecord)
              .WithMany(lr => lr.Fines)
              .HasForeignKey(f => f.LoanId)
              .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<UserActivity>()
                .HasOne(ua => ua.User)
                .WithMany()
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BookRating>()
                .HasOne(br => br.User)
                .WithMany()
                .HasForeignKey(br => br.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BookRating>()
                .HasOne(br => br.Book)
                .WithMany()
                .HasForeignKey(br => br.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            // CategoryPreference: composite key and relationship
            modelBuilder.Entity<CategoryPreference>()
                .HasKey(cp => new { cp.UserId, cp.CategoryId });

            modelBuilder.Entity<CategoryPreference>()
                .HasOne(cp => cp.User)
                .WithMany()
                .HasForeignKey(cp => cp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CategoryPreference>()
                .HasOne(cp => cp.Category)
                .WithMany()
                .HasForeignKey(cp => cp.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // AuthorPreference: composite key and relationship
            modelBuilder.Entity<AuthorPreference>()
                .HasKey(ap => new { ap.UserId, ap.AuthorId });

            modelBuilder.Entity<AuthorPreference>()
                .HasOne(ap => ap.User)
                .WithMany()
                .HasForeignKey(ap => ap.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AuthorPreference>()
                .HasOne(ap => ap.Author)
                .WithMany()
                .HasForeignKey(ap => ap.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);

            // LanguagePreference: composite key and relationship
            modelBuilder.Entity<LanguagePreference>()
                .HasKey(lp => new { lp.UserId, lp.LanguageId });

            modelBuilder.Entity<LanguagePreference>()
                .HasOne(lp => lp.User)
                .WithMany()
                .HasForeignKey(lp => lp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LanguagePreference>()
                .HasOne(lp => lp.Language)
                .WithMany()
                .HasForeignKey(lp => lp.LanguageId)
                .OnDelete(DeleteBehavior.Cascade);



            // Indexes
            modelBuilder.Entity<LoanRecord>().HasIndex(lr => lr.UserId);
            modelBuilder.Entity<LoanRecord>().HasIndex(lr => lr.DueDate);
            modelBuilder.Entity<Fine>().HasIndex(f => f.UserId);
            modelBuilder.Entity<Fine>().HasIndex(f => f.IsPaid);
            modelBuilder.Entity<UserActivity>().HasIndex(ua => ua.UserId);
            modelBuilder.Entity<UserActivity>().HasIndex(ua => ua.Timestamp);
            modelBuilder.Entity<BookRating>().HasIndex(br => br.UserId);
            modelBuilder.Entity<BookRating>().HasIndex(br => br.RatedOn);
            modelBuilder.Entity<CategoryPreference>().HasIndex(cp => new { cp.UserId, cp.CategoryId }).IsUnique();
            modelBuilder.Entity<AuthorPreference>().HasIndex(ap => new { ap.UserId, ap.AuthorId }).IsUnique();
            modelBuilder.Entity<LanguagePreference>().HasIndex(lp => new { lp.UserId, lp.LanguageId }).IsUnique();

        }
    }
}
