using PrivateLMS.Models;

public class CategoryPreference
{
    public int UserId { get; set; } 
    public int CategoryId { get; set; }

    public ApplicationUser? User { get; set; }
    public Category? Category { get; set; }
}
