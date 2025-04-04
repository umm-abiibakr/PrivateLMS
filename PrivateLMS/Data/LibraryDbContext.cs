using Microsoft.EntityFrameworkCore;
using PrivateLMS.Models;

namespace PrivateLMS.Data
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
        {
        }

        // Seed initial data
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>().HasData(
                new Book
                {
                    BookId = 1,
                    Title = "The Three Fundamental Principles",
                    Author = "Muhammad Ibn Abdil-Wahhab",
                    ISBN = "978-0201616224",
                    Language = "Arabic",
                    PublishedDate = new DateTime(2021, 10, 30),
                    IsAvailable = true
                },
                new Book
                {
                    BookId = 2,
                    Title = "Arbaun An-Nawawi",
                    Author = "Imaam An-Nawawi",
                    ISBN = "978-0132350884",
                    Language = "Hausa",
                    PublishedDate = new DateTime(2023, 8, 1),
                    IsAvailable = true
                },
                new Book
                {
                    BookId = 3,
                    Title = "Tafseer At-Tabari",
                    Author = "Imaam At-Tabari",
                    ISBN = "978-0451616235",
                    Language = "Arabic",
                    PublishedDate = new DateTime(2022, 11, 22),
                    IsAvailable = true
                },
                new Book
                {
                    BookId = 4,
                    Title = "Tafseer Ibn Kathir",
                    Author = "Ibn Kathir",
                    ISBN = "978-4562350123",
                    Language = "English",
                    PublishedDate = new DateTime(2020, 8, 15),
                    IsAvailable = true
                }
            );
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<LoanRecord> LoanRecords { get; set; }
        public DbSet<Publisher> Publishers { get; set; }
        public DbSet<Category> Categories { get; set; }


    }
}

