namespace PrivateLMS.ViewModels
{
    public class ReturnViewModel
    {
        public int LoanRecordId { get; set; }
        public string? BookTitle { get; set; }
        public string? LoanerName { get; set; }
        public DateTime? LoanDate { get; set; }
        public DateTime? DueDate { get; set; } 
    }
}