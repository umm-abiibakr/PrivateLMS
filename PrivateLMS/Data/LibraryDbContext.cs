using Microsoft.EntityFrameworkCore;
using PrivateLMS.Models;

namespace PrivateLMS.Data
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>().HasData(
        new Book
        {
            BookId = 1,
            Title = "The Three Fundamental Principles",
            AuthorId = 1,
            ISBN = "978-0201616224",
            Language = "Arabic",
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
            Language = "Hausa",
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
            Language = "Arabic",
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
            Language = "English",
            PublishedDate = new DateTime(2020, 8, 15),
            Description = "A widely respected Quranic commentary.",
            AvailableCopies = 4,
            IsAvailable = true,
            PublisherId = 4
            }
           );

            // Seed Authors
            modelBuilder.Entity<Author>().HasData(
                new Author { AuthorId = 1, Name = "Muhammad Ibn Abdil-Wahhab" },
                new Author { AuthorId = 2, Name = "Imaam An-Nawawi" },
                new Author { AuthorId = 3, Name = "Imaam At-Tabari" },
                new Author { AuthorId = 4, Name = "Ibn Kathir" }
            );

            // Seed User data 
            modelBuilder.Entity<User>().HasData(
                new User { UserId = 1, Username = "admin", Password = "password", ConfirmPassword = "password", FirstName = "Admin", LastName = "User", Gender = "Male", DateOfBirth = new DateTime(1990, 1, 1), Email = "admin@privatelms.com", PhoneNumber = "1234567890", Address = "123 Admin St.", City = "Admin City", State = "Admin State", PostalCode = "12345", Country = "Admin Country", Role = "Admin", TermsAccepted = true }
            );

            // Seed Publisher data
            modelBuilder.Entity<Publisher>().HasData(
                new Publisher { PublisherId = 1, PublisherName = "Dar Al-Ifta", Location = "Riyadh" },
                new Publisher { PublisherId = 2, PublisherName = "Islamic Heritage Press", Location = "Kano" },
                new Publisher { PublisherId = 3, PublisherName = "Tabari Publications", Location = "Cairo" },
                new Publisher { PublisherId = 4, PublisherName = "Kathir Books", Location = "London" }
            );

            // Seed Category data
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, CategoryName = "Islamic Principles" },
                new Category { CategoryId = 2, CategoryName = "Hadith" },
                new Category { CategoryId = 3, CategoryName = "Tafsir" }
            );

            // Seed BookCategory data
            modelBuilder.Entity<BookCategory>().HasData(
                new BookCategory { BookId = 1, CategoryId = 1 },
                new BookCategory { BookId = 2, CategoryId = 2 },
                new BookCategory { BookId = 3, CategoryId = 3 },
                new BookCategory { BookId = 4, CategoryId = 3 }
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
                .OnDelete(DeleteBehavior.SetNull); // If Publisher is deleted, set PublisherId to null

            modelBuilder.Entity<Book>()
            .HasOne(b => b.Author)
            .WithMany(a => a.Books)
            .HasForeignKey(b => b.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
        }

        public DbSet<Author> Authors { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<LoanRecord> LoanRecords { get; set; }
        public DbSet<Publisher> Publishers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<User> Users { get; set; }
    }
}