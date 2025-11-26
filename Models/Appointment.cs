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
        public string PatientId { get; set; }

        [ForeignKey(nameof(PatientId))]
        public Patient Patient { get; set; }

        [Required]
        public string DoctorId { get; set; }

        [ForeignKey(nameof(DoctorId))]
        public Doctor Doctor { get; set; }

        public DateTime AppointmentDate { get; set; }

        [StringLength(20)]
        public string Status { get; set; }   // Pending, Completed, Cancelled

        public string Reason { get; set; }
        public string Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // One-to-One
        public MedicalRecord MedicalRecord { get; set; }
    }
}
