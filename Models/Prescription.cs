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
        public string MedicalRecordId { get; set; } = string.Empty;

        [ForeignKey(nameof(MedicalRecordId))]
        public MedicalRecord MedicalRecord { get; set; } = null!;

        [Required]
        public string PatientId { get; set; } = string.Empty;

        [ForeignKey(nameof(PatientId))]
        public Patient Patient { get; set; } = null!;

        [Required]
        public string DoctorId { get; set; } = string.Empty;

        [ForeignKey(nameof(DoctorId))]
        public Doctor Doctor { get; set; } = null!;

        [Required, StringLength(100)]
        public string MedicineName { get; set; } = string.Empty;

        [StringLength(100)]
        public string Dosage { get; set; } = string.Empty;

        [StringLength(50)]
        public string Duration { get; set; } = string.Empty;

        public string Instructions { get; set; } = string.Empty;

        public DateTime PrescriptionDate { get; set; } = DateTime.Now;
    }
}
