using System;
using System.ComponentModel.DataAnnotations;

namespace PrivateLMS.ViewModels
{
    public class FineViewModel
    {
        public int LoanRecordId { get; set; }

        public string BookTitle { get; set; }

        public string LoanerName { get; set; }

        [DataType(DataType.Date)]
        public DateTime LoanDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ReturnDate { get; set; }

        [DataType(DataType.Currency)]
        public decimal FineAmount { get; set; }

        public bool IsFinePaid { get; set; }
    }
}