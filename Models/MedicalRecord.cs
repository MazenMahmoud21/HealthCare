using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthcareSystem.Models
{
    public class MedicalRecord
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

        [Required]
        public string AppointmentId { get; set; } = string.Empty;

        [ForeignKey(nameof(AppointmentId))]
        public Appointment Appointment { get; set; } = null!;

        public DateTime VisitDate { get; set; } = DateTime.Now;

        public string Diagnosis { get; set; } = string.Empty;
        public string Symptoms { get; set; } = string.Empty;
        public string Treatment { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // One-to-Many
        public ICollection<Prescription>? Prescriptions { get; set; }
    }
}
