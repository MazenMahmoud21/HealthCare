using System;
using System.ComponentModel.DataAnnotations;

namespace HealthcareSystem.Models
{
    public class User
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required, EmailAddress, StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(255)]
        public string Password { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string Role { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation (One-to-One)
        public Patient? Patient { get; set; }
        public Doctor? Doctor { get; set; }
    }
}
