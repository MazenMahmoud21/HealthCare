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
            var role = HttpContext.Session.GetString("Role") ?? string.Empty;
            string userId = HttpContext.Session.GetString("UserId") ?? string.Empty;

            if (role != "Admin" && role != "Patient")
                return Unauthorized();

            // Remove model state validation for navigation properties
            ModelState.Remove("Patient");
            ModelState.Remove("Doctor");

            if (!ModelState.IsValid)
            {
                ViewBag.Patients = _context.Patients.Include(p => p.User).ToList();
                ViewBag.Doctors = _context.Doctors.Include(d => d.User).ToList();
                return View(appointment);
            }

            if (role == "Patient")
            {
                var patient = _context.Patients.FirstOrDefault(p => p.UserId == userId);
                if (patient == null)
                    return Unauthorized();

                appointment.PatientId = patient.Id; // string
            }

            appointment.Id = Guid.NewGuid().ToString();
            appointment.Status = "Scheduled";
            appointment.CreatedAt = DateTime.Now;

            _context.Appointments.Add(appointment);
            _context.SaveChanges();

            if (role == "Patient")
                return RedirectToAction("MyAppointments");
            
            return RedirectToAction("Index");
        }

        // -------------------------------------
        // APPOINTMENT DETAILS (Admin, Doctor, Patient)
        // -------------------------------------
        public IActionResult Details(string id)
        {
            string userId = HttpContext.Session.GetString("UserId") ?? string.Empty;
            var role = HttpContext.Session.GetString("Role") ?? string.Empty;

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

        // -------------------------------------
        // CANCEL APPOINTMENT (Patient)
        // -------------------------------------
        [HttpPost]
        public IActionResult Cancel(string id)
        {
            var role = HttpContext.Session.GetString("Role");
            var userId = HttpContext.Session.GetString("UserId");

            var appointment = _context.Appointments
                .Include(a => a.Patient)
                .FirstOrDefault(a => a.Id == id);

            if (appointment == null)
                return NotFound();

            // Only patient who owns the appointment can cancel
            if (role == "Patient" && appointment.Patient?.UserId != userId)
                return Unauthorized();

            // Admin can also cancel
            if (role != "Admin" && role != "Patient")
                return Unauthorized();

            appointment.Status = "Cancelled";
            _context.SaveChanges();

            TempData["Success"] = "Appointment cancelled successfully.";
            
            if (role == "Patient")
                return RedirectToAction("PatientDashboard", "Home");
            
            return RedirectToAction("Index");
        }

        // -------------------------------------
        // COMPLETE APPOINTMENT (Doctor)
        // -------------------------------------
        [HttpPost]
        public IActionResult Complete(string id)
        {
            var role = HttpContext.Session.GetString("Role");
            var userId = HttpContext.Session.GetString("UserId");

            if (role != "Doctor")
                return Unauthorized();

            var appointment = _context.Appointments
                .Include(a => a.Doctor)
                .FirstOrDefault(a => a.Id == id);

            if (appointment == null)
                return NotFound();

            // Only the assigned doctor can complete
            if (appointment.Doctor?.UserId != userId)
                return Unauthorized();

            appointment.Status = "Completed";
            _context.SaveChanges();

            TempData["Success"] = "Appointment marked as completed.";
            return RedirectToAction("Details", new { id = id });
        }

        // -------------------------------------
        // CALENDAR VIEW (Admin/Doctor)
        // -------------------------------------
        public IActionResult Calendar()
        {
            var role = HttpContext.Session.GetString("Role");
            var userId = HttpContext.Session.GetString("UserId");

            if (role != "Admin" && role != "Doctor")
                return Unauthorized();

            IQueryable<Appointment> query = _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor);

            if (role == "Doctor")
            {
                var doctor = _context.Doctors.FirstOrDefault(d => d.UserId == userId);
                if (doctor != null)
                {
                    query = query.Where(a => a.DoctorId == doctor.Id);
                }
            }

            var appointments = query.ToList();
            return View(appointments);
        }

        // -------------------------------------
        // MY APPOINTMENTS (Patient)
        // -------------------------------------
        public IActionResult MyAppointments()
        {
            var role = HttpContext.Session.GetString("Role");
            var userId = HttpContext.Session.GetString("UserId");

            if (role != "Patient")
                return RedirectToAction("Index", "Home");

            var patient = _context.Patients.FirstOrDefault(p => p.UserId == userId);
            if (patient == null)
                return RedirectToAction("Index", "Home");

            var appointments = _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Where(a => a.PatientId == patient.Id)
                .OrderByDescending(a => a.AppointmentDate)
                .ToList();

            return View(appointments);
        }
    }
}
