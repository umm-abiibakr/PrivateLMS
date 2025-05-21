using System;

namespace PrivateLMS.ViewModels
{
    public class BookReviewViewModel
    {
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Review { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime RatedOn { get; set; }
    }
}