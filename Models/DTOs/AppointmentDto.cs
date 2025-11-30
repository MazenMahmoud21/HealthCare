using System.ComponentModel.DataAnnotations;

namespace HealthcareSystem.Models.DTOs
{
    public class AppointmentDto
    {
        [Required(ErrorMessage = "Patient is required")]
        public string PatientId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Doctor is required")]
        public string DoctorId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Appointment date is required")]
        [DataType(DataType.DateTime)]
        public DateTime AppointmentDate { get; set; }

        [Required(ErrorMessage = "Reason is required")]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Notes { get; set; }
    }
}
