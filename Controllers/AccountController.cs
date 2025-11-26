using HealthcareSystem.Data;
using HealthcareSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

namespace HealthcareSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly HealthcareDbContext _context;
        private readonly IPasswordHasher<User> _hasher;

        public AccountController(HealthcareDbContext context, IPasswordHasher<User> hasher)
        {
            _context = context;
            _hasher = hasher;
        }

        // LOGIN
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            var result = _hasher.VerifyHashedPassword(user, user.Password, password);
            if (result == PasswordVerificationResult.Failed)
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            // Saving session (string ID)
            HttpContext.Session.SetString("UserId", user.Id);
            HttpContext.Session.SetString("Role", user.Role);

            // Redirect by role
            return user.Role switch
            {
                "Admin" => RedirectToAction("AdminDashboard", "Home"),
                "Doctor" => RedirectToAction("DoctorDashboard", "Home"),
                "Patient" => RedirectToAction("PatientDashboard", "Home"),
                _ => RedirectToAction("Index", "Home"),
            };
        }

        // REGISTER (Patient self-register)
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User user, Patient patient)
        {
            if (!ModelState.IsValid) return View();

            // Set role to Patient
            user.Role = "Patient";
            user.Password = _hasher.HashPassword(user, user.Password);
            user.CreatedAt = DateTime.Now;

            _context.Users.Add(user);
            _context.SaveChanges();

            // Create Patient Profile
            patient.UserId = user.Id; // string assignment
            patient.CreatedAt = DateTime.Now;

            _context.Patients.Add(patient);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

        // LOGOUT
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
