using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthcareSystem.Models
{
    public class Prescription
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string MedicalRecordId { get; set; }

        [ForeignKey(nameof(MedicalRecordId))]
        public MedicalRecord MedicalRecord { get; set; }

        [Required]
        public string PatientId { get; set; }

        [ForeignKey(nameof(PatientId))]
        public Patient Patient { get; set; }

        [Required]
        public string DoctorId { get; set; }

        [ForeignKey(nameof(DoctorId))]
        public Doctor Doctor { get; set; }

        [Required, StringLength(100)]
        public string MedicineName { get; set; }

        [StringLength(100)]
        public string Dosage { get; set; }

        [StringLength(50)]
        public string Duration { get; set; }

        public string Instructions { get; set; }

        public DateTime PrescriptionDate { get; set; } = DateTime.Now;
    }
}
