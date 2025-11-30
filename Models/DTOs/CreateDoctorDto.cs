using System.ComponentModel.DataAnnotations;

namespace HealthcareSystem.Models.DTOs
{
    public class CreateDoctorDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Specialization is required")]
        [StringLength(100)]
        public string Specialization { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number")]
        public string? Phone { get; set; }

        [Range(0, 10000, ErrorMessage = "Consultation fee must be between 0 and 10000")]
        public double ConsultationFee { get; set; }
    }
}
