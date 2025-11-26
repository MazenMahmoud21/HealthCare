using HealthcareSystem.Data;
using HealthcareSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthcareSystem.Controllers
{
    public class AppointmentController : Controller
    {
        private readonly HealthcareDbContext _context;

        public AppointmentController(HealthcareDbContext context)
        {
            _context = context;
        }

        // -------------------------------------
        // LIST OF APPOINTMENTS (Admin only)
        // -------------------------------------
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return Unauthorized();

            var appointments = _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .ToList();

            return View(appointments);
        }

        // -------------------------------------
        // CREATE APPOINTMENT
        // Admin schedules for any patient
        // Patient schedules for themselves
        // -------------------------------------
        [HttpGet]
        public IActionResult Create()
        {
            var role = HttpContext.Session.GetString("Role");

            if (role != "Admin" && role != "Patient")
                return Unauthorized();

            ViewBag.Patients = _context.Patients
                .Include(p => p.User)
                .ToList();

            ViewBag.Doctors = _context.Doctors
                .Include(d => d.User)
                .ToList();

            return View();
        }

        [HttpPost]
        public IActionResult Create(Appointment appointment)
        {
            var role = HttpContext.Session.GetString("Role");
            string userId = HttpContext.Session.GetString("UserId");

            if (role != "Admin" && role != "Patient")
                return Unauthorized();

            if (!ModelState.IsValid)
            {
                ViewBag.Patients = _context.Patients.Include(p => p.User).ToList();
                ViewBag.Doctors = _context.Doctors.Include(d => d.User).ToList();
                return View();
            }

            if (role == "Patient")
            {
                var patient = _context.Patients.FirstOrDefault(p => p.UserId == userId);
                if (patient == null)
                    return Unauthorized();

                appointment.PatientId = patient.Id; // string
            }

            appointment.Status = "Scheduled";
            appointment.CreatedAt = DateTime.Now;

            // Generate string ID (GUID)
            appointment.Id = Guid.NewGuid().ToString();

            _context.Appointments.Add(appointment);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // -------------------------------------
        // APPOINTMENT DETAILS (Admin, Doctor, Patient)
        // -------------------------------------
        public IActionResult Details(string id)
        {
            string userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("Role");

            var appointment = _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .FirstOrDefault(a => a.Id == id);

            if (appointment == null)
                return NotFound();

            if (role == "Patient" && appointment.Patient.UserId != userId)
                return Unauthorized();

            if (role == "Doctor" && appointment.Doctor.UserId != userId)
                return Unauthorized();

            return View(appointment);
        }

        // -------------------------------------
        // EDIT APPOINTMENT (Admin only)
        // -------------------------------------
        [HttpGet]
        public IActionResult Edit(string id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return Unauthorized();

            var appointment = _context.Appointments.FirstOrDefault(a => a.Id == id);

            if (appointment == null)
                return NotFound();

            ViewBag.Patients = _context.Patients.Include(p => p.User).ToList();
            ViewBag.Doctors = _context.Doctors.Include(d => d.User).ToList();

            return View(appointment);
        }

        [HttpPost]
        public IActionResult Edit(Appointment updated)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return Unauthorized();

            var appointment = _context.Appointments.FirstOrDefault(a => a.Id == updated.Id);

            if (appointment == null)
                return NotFound();

            appointment.PatientId = updated.PatientId;
            appointment.DoctorId = updated.DoctorId;
            appointment.AppointmentDate = updated.AppointmentDate;
            appointment.Status = updated.Status;
            appointment.Reason = updated.Reason;
            appointment.Notes = updated.Notes;

            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // -------------------------------------
        // DELETE APPOINTMENT (Admin only)
        // -------------------------------------
        [HttpPost]
        public IActionResult Delete(string id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return Unauthorized();

            var appointment = _context.Appointments.FirstOrDefault(a => a.Id == id);

            if (appointment == null)
                return NotFound();

            _context.Appointments.Remove(appointment);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
