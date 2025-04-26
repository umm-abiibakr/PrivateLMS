using System;
using System.ComponentModel.DataAnnotations;

namespace PrivateLMS.Models
{
    public class UserActivity
    {
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public string Action { get; set; }
        [Required]
        public DateTime Timestamp { get; set; }
        public string Details { get; set; }
        public string? IPAddress { get; set; }
        public string? SessionId { get; set; }
        public int? EntityId { get; set; }
        public string? EntityType { get; set; }
        public string? Status { get; set; }
        public ApplicationUser User { get; set; }
    }
}