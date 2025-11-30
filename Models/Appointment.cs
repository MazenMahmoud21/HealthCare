using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthcareSystem.Models
{
    public class Appointment
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string PatientId { get; set; } = string.Empty;

        [ForeignKey(nameof(PatientId))]
        public Patient Patient { get; set; } = null!;

        [Required]
        public string DoctorId { get; set; } = string.Empty;

        [ForeignKey(nameof(DoctorId))]
        public Doctor Doctor { get; set; } = null!;

        public DateTime AppointmentDate { get; set; }

        [StringLength(20)]
        public string Status { get; set; } = string.Empty;

        public string Reason { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // One-to-One
        public MedicalRecord? MedicalRecord { get; set; }
    }
}
