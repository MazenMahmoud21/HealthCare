using HealthcareSystem.Data;
using HealthcareSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace HealthcareSystem.Controllers
{
    public class PatientController : Controller
    {
        private readonly HealthcareDbContext _context;
        private readonly IPasswordHasher<User> _hasher;

        public PatientController(HealthcareDbContext context, IPasswordHasher<User> hasher)
        {
            _context = context;
            _hasher = hasher;
        }

        // -------------------------------------
        // PATIENT LIST (Admin only)
        // -------------------------------------
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return Unauthorized();

            var patients = _context.Patients
                .Include(p => p.User)
                .ToList();

            return View(patients);
        }

        // -------------------------------------
        // CREATE PATIENT (Admin - Manual Register)
        // -------------------------------------
        [HttpGet]
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return Unauthorized();

            return View();
        }

        [HttpPost]
        public IActionResult Create(User user, Patient patient)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return Unauthorized();

            if (!ModelState.IsValid)
                return View();

            // Use string IDs
            user.Id = Guid.NewGuid().ToString();
            user.Role = "Patient";
            user.Password = _hasher.HashPassword(user, user.Password);
            user.CreatedAt = DateTime.Now;

            _context.Users.Add(user);
            _context.SaveChanges();

            patient.Id = Guid.NewGuid().ToString();
            patient.UserId = user.Id;
            patient.CreatedAt = DateTime.Now;

            _context.Patients.Add(patient);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // -------------------------------------
        // PATIENT DETAILS (Admin + Patient)
        // Includes: profile + appointments + records + prescriptions
        // -------------------------------------
        public IActionResult Details(string id)
        {
            var role = HttpContext.Session.GetString("Role");
            string userId = HttpContext.Session.GetString("UserId");

            var patient = _context.Patients
                .Include(p => p.User)
                .Include(p => p.Appointments)
                    .ThenInclude(a => a.Doctor)
                        .ThenInclude(d => d.User)
                .Include(p => p.MedicalRecords)
                    .ThenInclude(m => m.Doctor)
                        .ThenInclude(d => d.User)
                .Include(p => p.Prescriptions)
                    .ThenInclude(pr => pr.Doctor)
                        .ThenInclude(d => d.User)
                .FirstOrDefault(p => p.Id == id);

            if (patient == null)
                return NotFound();

            if (role == "Patient" && patient.UserId != userId)
                return Unauthorized();

            return View(patient);
        }

        // -------------------------------------
        // EDIT PROFILE (Patient)
        // -------------------------------------
        [HttpGet]
        public IActionResult Edit()
        {
            if (HttpContext.Session.GetString("Role") != "Patient")
                return Unauthorized();

            string userId = HttpContext.Session.GetString("UserId");

            var patient = _context.Patients.FirstOrDefault(p => p.UserId == userId);

            return View(patient);
        }

        [HttpPost]
        public IActionResult Edit(Patient updated)
        {
            if (HttpContext.Session.GetString("Role") != "Patient")
                return Unauthorized();

            var patient = _context.Patients.FirstOrDefault(p => p.Id == updated.Id);

            if (patient == null)
                return NotFound();

            patient.FirstName = updated.FirstName;
            patient.LastName = updated.LastName;
            patient.Phone = updated.Phone;
            patient.Address = updated.Address;
            patient.BloodGroup = updated.BloodGroup;

            _context.SaveChanges();

            return RedirectToAction("Details", new { id = patient.Id });
        }
    }
}
