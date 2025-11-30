using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HealthcareSystem.Data;
using HealthcareSystem.Models;

namespace HealthcareSystem.Controllers
{
    public class PrescriptionController : Controller
    {
        private readonly HealthcareDbContext _context;

        public PrescriptionController(HealthcareDbContext context)
        {
            _context = context;
        }

        // GET: Prescription - Admin/Doctor view all prescriptions
        public async Task<IActionResult> Index()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin" && role != "Doctor")
            {
                return RedirectToAction("Index", "Home");
            }

            IQueryable<Prescription> query = _context.Prescriptions
                .Include(p => p.Patient)
                .Include(p => p.Doctor)
                .Include(p => p.MedicalRecord);

            if (role == "Doctor")
            {
                var userId = HttpContext.Session.GetString("UserId");
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
                if (doctor == null) return RedirectToAction("Index", "Home");
                
                query = query.Where(p => p.DoctorId == doctor.Id);
            }

            var prescriptions = await query.OrderByDescending(p => p.PrescriptionDate).ToListAsync();
            return View(prescriptions);
        }

        // GET: Prescription/MyPrescriptions - Patient view their prescriptions
        public async Task<IActionResult> MyPrescriptions()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Patient")
            {
                return RedirectToAction("Index", "Home");
            }

            var userId = HttpContext.Session.GetString("UserId");
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
            if (patient == null) return RedirectToAction("Index", "Home");

            var prescriptions = await _context.Prescriptions
                .Include(p => p.Doctor)
                .Include(p => p.MedicalRecord)
                .Where(p => p.PatientId == patient.Id)
                .OrderByDescending(p => p.PrescriptionDate)
                .ToListAsync();

            ViewBag.Patient = patient;
            return View(prescriptions);
        }

        // GET: Prescription/Create
        public async Task<IActionResult> Create(string? recordId)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Doctor")
            {
                return RedirectToAction("Index", "Home");
            }

            var userId = HttpContext.Session.GetString("UserId");
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null) return RedirectToAction("Index", "Home");

            if (string.IsNullOrEmpty(recordId))
            {
                // Show list of medical records to select from
                var records = await _context.MedicalRecords
                    .Include(r => r.Patient)
                    .Where(r => r.DoctorId == doctor.Id)
                    .OrderByDescending(r => r.VisitDate)
                    .ToListAsync();
                ViewBag.Records = records;
                return View(new Prescription());
            }

            var record = await _context.MedicalRecords
                .Include(r => r.Patient)
                .FirstOrDefaultAsync(r => r.Id == recordId && r.DoctorId == doctor.Id);

            if (record == null)
            {
                TempData["Error"] = "Medical record not found.";
                return RedirectToAction("Index", "MedicalRecord");
            }

            ViewBag.Record = record;
            ViewBag.Doctor = doctor;

            var prescription = new Prescription
            {
                MedicalRecordId = record.Id,
                PatientId = record.PatientId,
                DoctorId = doctor.Id,
                PrescriptionDate = DateTime.Now
            };

            return View(prescription);
        }

        // POST: Prescription/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Prescription prescription)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Doctor")
            {
                return RedirectToAction("Index", "Home");
            }

            var userId = HttpContext.Session.GetString("UserId");
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null) return RedirectToAction("Index", "Home");

            // Verify the doctor owns this medical record
            var record = await _context.MedicalRecords
                .Include(r => r.Patient)
                .FirstOrDefaultAsync(r => r.Id == prescription.MedicalRecordId && r.DoctorId == doctor.Id);

            if (record == null)
            {
                TempData["Error"] = "Medical record not found.";
                return RedirectToAction("Index", "MedicalRecord");
            }

            prescription.Id = Guid.NewGuid().ToString();
            prescription.DoctorId = doctor.Id;
            prescription.PatientId = record.PatientId;
            prescription.PrescriptionDate = DateTime.Now;

            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Prescription created successfully!";
            return RedirectToAction("Details", "MedicalRecord", new { id = prescription.MedicalRecordId });
        }

        // GET: Prescription/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var prescription = await _context.Prescriptions
                .Include(p => p.Patient)
                    .ThenInclude(p => p!.User)
                .Include(p => p.Doctor)
                .Include(p => p.MedicalRecord)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (prescription == null) return NotFound();

            // Access control
            var role = HttpContext.Session.GetString("Role");
            var userId = HttpContext.Session.GetString("UserId");

            if (role == "Patient")
            {
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
                if (patient == null || prescription.PatientId != patient.Id)
                    return Forbid();
            }
            else if (role == "Doctor")
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
                if (doctor == null || prescription.DoctorId != doctor.Id)
                    return Forbid();
            }
            else if (role != "Admin")
            {
                return RedirectToAction("Index", "Home");
            }

            return View(prescription);
        }

        // GET: Prescription/Print/5
        public async Task<IActionResult> Print(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var prescription = await _context.Prescriptions
                .Include(p => p.Patient)
                    .ThenInclude(p => p!.User)
                .Include(p => p.Doctor)
                .Include(p => p.MedicalRecord)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (prescription == null) return NotFound();

            // Access control
            var role = HttpContext.Session.GetString("Role");
            var userId = HttpContext.Session.GetString("UserId");

            if (role == "Patient")
            {
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
                if (patient == null || prescription.PatientId != patient.Id)
                    return Forbid();
            }
            else if (role == "Doctor")
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
                if (doctor == null || prescription.DoctorId != doctor.Id)
                    return Forbid();
            }
            else if (role != "Admin")
            {
                return RedirectToAction("Index", "Home");
            }

            return View(prescription);
        }

        // GET: Prescription/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Doctor")
            {
                return RedirectToAction("Index", "Home");
            }

            if (string.IsNullOrEmpty(id)) return NotFound();

            var userId = HttpContext.Session.GetString("UserId");
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null) return RedirectToAction("Index", "Home");

            var prescription = await _context.Prescriptions
                .Include(p => p.Patient)
                .Include(p => p.MedicalRecord)
                .FirstOrDefaultAsync(p => p.Id == id && p.DoctorId == doctor.Id);

            if (prescription == null) return NotFound();

            ViewBag.Record = prescription.MedicalRecord;
            return View(prescription);
        }

        // POST: Prescription/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Prescription prescription)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Doctor")
            {
                return RedirectToAction("Index", "Home");
            }

            if (id != prescription.Id) return NotFound();

            var userId = HttpContext.Session.GetString("UserId");
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null) return RedirectToAction("Index", "Home");

            var existingPrescription = await _context.Prescriptions
                .FirstOrDefaultAsync(p => p.Id == id && p.DoctorId == doctor.Id);

            if (existingPrescription == null) return NotFound();

            existingPrescription.MedicineName = prescription.MedicineName;
            existingPrescription.Dosage = prescription.Dosage;
            existingPrescription.Duration = prescription.Duration;
            existingPrescription.Instructions = prescription.Instructions;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Prescription updated successfully!";
            return RedirectToAction("Details", new { id = prescription.Id });
        }

        // POST: Prescription/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id, string recordId)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Doctor")
            {
                return RedirectToAction("Index", "Home");
            }

            var userId = HttpContext.Session.GetString("UserId");
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null) return RedirectToAction("Index", "Home");

            var prescription = await _context.Prescriptions
                .FirstOrDefaultAsync(p => p.Id == id && p.DoctorId == doctor.Id);

            if (prescription != null)
            {
                _context.Prescriptions.Remove(prescription);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Prescription deleted successfully!";
            }

            return RedirectToAction("Details", "MedicalRecord", new { id = recordId });
        }
    }
}
