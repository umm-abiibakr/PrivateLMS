using PrivateLMS.Models;

public class LanguagePreference
{
    public int UserId { get; set; } 
    public int LanguageId { get; set; }

    public ApplicationUser? User { get; set; }
    public Language? Language { get; set; }
}
