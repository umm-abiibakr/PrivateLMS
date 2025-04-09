namespace PrivateLMS.ViewModels
{
    public class ReturnViewModel
    {
        public int LoanRecordId { get; set; }
        public string? BookTitle { get; set; }
        public int UserId { get; set; } 
        public string? LoanerName { get; set; } // For display only, derived from User
        public DateTime? LoanDate { get; set; }
        public DateTime? DueDate { get; set; }
    }
}