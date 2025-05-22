namespace PrivateLMS.Models
{
    public class BookCategory
    {
        public int BookId { get; set; }
        public Book Book { get; set; } = null!; // Non-nullable 

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!; // Non-nullable 
    }
}