using PrivateLMS.Models;

public class AuthorPreference
{
    public int UserId { get; set; } 
    public int AuthorId { get; set; }

    public ApplicationUser? User { get; set; }
    public Author? Author { get; set; }
}
