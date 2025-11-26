using HealthcareSystem.Data;
using HealthcareSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace HealthcareSystem.Controllers
{
    public class DoctorController : Controller
    {
        private readonly HealthcareDbContext _context;
        private readonly IPasswordHasher<User> _hasher;

        public DoctorController(HealthcareDbContext context, IPasswordHasher<User> hasher)
        {
            _context = context;
            _hasher = hasher;
        }

        // --------------------------------------
        // DOCTOR LIST (Admin & Patient)
        // Patients use this to browse doctors
        // --------------------------------------
        public IActionResult Index()
        {
            var doctors = _context.Doctors
                .Include(d => d.User)
                .ToList();

            return View(doctors);
        }

        // --------------------------------------
        // CREATE DOCTOR (Admin only)
        // --------------------------------------
        [HttpGet]
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return Unauthorized();

            return View();
        }

        [HttpPost]
        public IActionResult Create(User user, Doctor doctor)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return Unauthorized();

            if (!ModelState.IsValid)
                return View();

            // Set role and hash password
            user.Role = "Doctor";
            user.Password = _hasher.HashPassword(user, user.Password);
            user.CreatedAt = DateTime.Now;

            // Generate string ID
            user.Id = Guid.NewGuid().ToString();
            _context.Users.Add(user);
            _context.SaveChanges();

            doctor.UserId = user.Id;
            doctor.Id = Guid.NewGuid().ToString(); // string ID
            doctor.CreatedAt = DateTime.Now;

            _context.Doctors.Add(doctor);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // --------------------------------------
        // DOCTOR DASHBOARD (Appointment summary)
        // --------------------------------------
        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetString("Role") != "Doctor")
                return Unauthorized();

            string userId = HttpContext.Session.GetString("UserId");
            var doctor = _context.Doctors.FirstOrDefault(d => d.UserId == userId);

            if (doctor == null)
                return Unauthorized();

            var appointments = _context.Appointments
                .Where(a => a.DoctorId == doctor.Id)
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .ToList();

            return View(appointments);
        }

        // --------------------------------------
        // DOCTOR DETAILS (Admin view)
        // --------------------------------------
        public IActionResult Details(string id)
        {
            var doctor = _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Appointments)
                    .ThenInclude(a => a.Patient)
                        .ThenInclude(p => p.User)
                .FirstOrDefault(d => d.Id == id);

            if (doctor == null)
                return NotFound();

            return View(doctor);
        }
    }
}
