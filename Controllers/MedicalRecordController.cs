using HealthcareSystem.Data;
using HealthcareSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthcareSystem.Controllers
{
    public class MedicalRecordController : Controller
    {
        private readonly HealthcareDbContext _context;

        public MedicalRecordController(HealthcareDbContext context)
        {
            _context = context;
        }

        // -------------------------------------
        // LIST ALL MEDICAL RECORDS (Admin/Doctor)
        // -------------------------------------
        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("Role");
            var userId = HttpContext.Session.GetString("UserId");

            if (role != "Admin" && role != "Doctor")
                return RedirectToAction("Login", "Account");

            IQueryable<MedicalRecord> query = _context.MedicalRecords
                .Include(m => m.Patient)
                .Include(m => m.Doctor)
                .Include(m => m.Appointment);

            // Doctors only see their own records
            if (role == "Doctor")
            {
                var doctor = _context.Doctors.FirstOrDefault(d => d.UserId == userId);
                if (doctor == null) return RedirectToAction("Login", "Account");
                query = query.Where(m => m.DoctorId == doctor.Id);
            }

            var records = query.OrderByDescending(m => m.VisitDate).ToList();
            return View(records);
        }

        // -------------------------------------
        // PATIENT'S OWN MEDICAL RECORDS
        // -------------------------------------
        public IActionResult MyRecords()
        {
            var role = HttpContext.Session.GetString("Role");
            var userId = HttpContext.Session.GetString("UserId");

            if (role != "Patient")
                return RedirectToAction("Login", "Account");

            var patient = _context.Patients.FirstOrDefault(p => p.UserId == userId);
            if (patient == null)
                return RedirectToAction("Login", "Account");

            var records = _context.MedicalRecords
                .Include(m => m.Doctor)
                .Include(m => m.Appointment)
                .Include(m => m.Prescriptions)
                .Where(m => m.PatientId == patient.Id)
                .OrderByDescending(m => m.VisitDate)
                .ToList();

            ViewBag.Patient = patient;
            return View(records);
        }

        // -------------------------------------
        // CREATE MEDICAL RECORD (Doctor only)
        // -------------------------------------
        [HttpGet]
        public IActionResult Create(string appointmentId)
        {
            var role = HttpContext.Session.GetString("Role");
            var userId = HttpContext.Session.GetString("UserId");

            if (role != "Doctor")
                return RedirectToAction("Login", "Account");

            var doctor = _context.Doctors.FirstOrDefault(d => d.UserId == userId);
            if (doctor == null)
                return RedirectToAction("Login", "Account");

            if (string.IsNullOrEmpty(appointmentId))
            {
                // Show list of appointments that don't have records yet
                ViewBag.Appointments = _context.Appointments
                    .Include(a => a.Patient)
                    .Where(a => a.DoctorId == doctor.Id && 
                               a.MedicalRecord == null &&
                               a.Status == "Scheduled")
                    .OrderByDescending(a => a.AppointmentDate)
                    .ToList();
                return View();
            }

            var appointment = _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefault(a => a.Id == appointmentId && a.DoctorId == doctor.Id);

            if (appointment == null)
                return NotFound();

            // Check if record already exists
            if (_context.MedicalRecords.Any(m => m.AppointmentId == appointmentId))
            {
                TempData["Error"] = "A medical record already exists for this appointment.";
                return RedirectToAction("Details", "Appointment", new { id = appointmentId });
            }

            ViewBag.Appointment = appointment;
            ViewBag.Doctor = doctor;

            var record = new MedicalRecord
            {
                AppointmentId = appointmentId,
                PatientId = appointment.PatientId,
                DoctorId = doctor.Id,
                VisitDate = DateTime.Now
            };

            return View(record);
        }

        [HttpPost]
        public IActionResult Create(MedicalRecord record)
        {
            var role = HttpContext.Session.GetString("Role");
            var userId = HttpContext.Session.GetString("UserId");

            if (role != "Doctor")
                return RedirectToAction("Login", "Account");

            var doctor = _context.Doctors.FirstOrDefault(d => d.UserId == userId);
            if (doctor == null)
                return RedirectToAction("Login", "Account");

            // Verify the appointment belongs to this doctor
            var appointment = _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefault(a => a.Id == record.AppointmentId && a.DoctorId == doctor.Id);

            if (appointment == null)
                return NotFound();

            record.Id = Guid.NewGuid().ToString();
            record.DoctorId = doctor.Id;
            record.PatientId = appointment.PatientId;
            record.CreatedAt = DateTime.Now;

            _context.MedicalRecords.Add(record);

            // Mark appointment as completed
            appointment.Status = "Completed";

            _context.SaveChanges();

            TempData["Success"] = "Medical record created successfully.";
            return RedirectToAction("Details", new { id = record.Id });
        }

        // -------------------------------------
        // VIEW MEDICAL RECORD DETAILS
        // -------------------------------------
        public IActionResult Details(string id)
        {
            var role = HttpContext.Session.GetString("Role");
            var userId = HttpContext.Session.GetString("UserId");

            var record = _context.MedicalRecords
                .Include(m => m.Patient)
                    .ThenInclude(p => p.User)
                .Include(m => m.Doctor)
                    .ThenInclude(d => d.User)
                .Include(m => m.Appointment)
                .Include(m => m.Prescriptions)
                .FirstOrDefault(m => m.Id == id);

            if (record == null)
                return NotFound();

            // Access control
            if (role == "Patient")
            {
                var patient = _context.Patients.FirstOrDefault(p => p.UserId == userId);
                if (patient == null || record.PatientId != patient.Id)
                    return Unauthorized();
            }
            else if (role == "Doctor")
            {
                var doctor = _context.Doctors.FirstOrDefault(d => d.UserId == userId);
                if (doctor == null || record.DoctorId != doctor.Id)
                    return Unauthorized();
            }
            else if (role != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            return View(record);
        }

        // -------------------------------------
        // EDIT MEDICAL RECORD (Doctor only)
        // -------------------------------------
        [HttpGet]
        public IActionResult Edit(string id)
        {
            var role = HttpContext.Session.GetString("Role");
            var userId = HttpContext.Session.GetString("UserId");

            if (role != "Doctor")
                return RedirectToAction("Login", "Account");

            var doctor = _context.Doctors.FirstOrDefault(d => d.UserId == userId);
            if (doctor == null)
                return RedirectToAction("Login", "Account");

            var record = _context.MedicalRecords
                .Include(m => m.Patient)
                .Include(m => m.Appointment)
                .FirstOrDefault(m => m.Id == id && m.DoctorId == doctor.Id);

            if (record == null)
                return NotFound();

            return View(record);
        }

        [HttpPost]
        public IActionResult Edit(MedicalRecord updated)
        {
            var role = HttpContext.Session.GetString("Role");
            var userId = HttpContext.Session.GetString("UserId");

            if (role != "Doctor")
                return RedirectToAction("Login", "Account");

            var doctor = _context.Doctors.FirstOrDefault(d => d.UserId == userId);
            if (doctor == null)
                return RedirectToAction("Login", "Account");

            var record = _context.MedicalRecords.FirstOrDefault(m => m.Id == updated.Id && m.DoctorId == doctor.Id);

            if (record == null)
                return NotFound();

            record.Symptoms = updated.Symptoms;
            record.Diagnosis = updated.Diagnosis;
            record.Treatment = updated.Treatment;
            record.Notes = updated.Notes;

            _context.SaveChanges();

            TempData["Success"] = "Medical record updated successfully.";
            return RedirectToAction("Details", new { id = record.Id });
        }
    }
}
