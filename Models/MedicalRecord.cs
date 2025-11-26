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
        public string PatientId { get; set; }

        [ForeignKey(nameof(PatientId))]
        public Patient Patient { get; set; }

        [Required]
        public string DoctorId { get; set; }

        [ForeignKey(nameof(DoctorId))]
        public Doctor Doctor { get; set; }

        [Required]
        public string AppointmentId { get; set; }

        [ForeignKey(nameof(AppointmentId))]
        public Appointment Appointment { get; set; }

        public DateTime VisitDate { get; set; } = DateTime.Now;

        public string Diagnosis { get; set; }
        public string Symptoms { get; set; }
        public string Treatment { get; set; }
        public string Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // One-to-Many
        public ICollection<Prescription> Prescriptions { get; set; }
    }
}
