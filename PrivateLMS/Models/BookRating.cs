namespace PrivateLMS.Models
{
    public class BookRating
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int BookId { get; set; }
        public int Rating { get; set; } // 1 to 5
        public string? Review { get; set; } 
        public DateTime RatedOn { get; set; }

        public ApplicationUser User { get; set; } = null!;
        public Book Book { get; set; } = null!;
    }
}