using HealthcareSystem.Data;
using HealthcareSystem.Models;
using HealthcareSystem.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HealthcareSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly HealthcareDbContext _context;
        private readonly IPasswordHasher<User> _hasher;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            HealthcareDbContext context, 
            IPasswordHasher<User> hasher,
            ILogger<AccountController> logger)
        {
            _context = context;
            _hasher = hasher;
            _logger = logger;
        }

        // LOGIN
        [HttpGet]
        public IActionResult Login()
        {
            // Redirect if already logged in
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
            {
                var role = HttpContext.Session.GetString("Role");
                return RedirectToDashboard(role);
            }

            return View(new LoginDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid email or password.");
                    return View(model);
                }

                var result = _hasher.VerifyHashedPassword(user, user.Password, model.Password);
                if (result == PasswordVerificationResult.Failed)
                {
                    ModelState.AddModelError(string.Empty, "Invalid email or password.");
                    return View(model);
                }

                // Set session
                HttpContext.Session.SetString("UserId", user.Id);
                HttpContext.Session.SetString("Role", user.Role);
                HttpContext.Session.SetString("UserEmail", user.Email);

                _logger.LogInformation($"User {user.Email} logged in successfully.");

                // Redirect by role
                return RedirectToDashboard(user.Role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again.");
                return View(model);
            }
        }

        // REGISTER (Patient self-register)
        [HttpGet]
        public IActionResult Register()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("UserId")))
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new RegisterDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Check if email already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());

                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "This email is already registered.");
                    return View(model);
                }

                // Create user
                var user = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = model.Email,
                    Role = "Patient",
                    CreatedAt = DateTime.Now
                };
                user.Password = _hasher.HashPassword(user, model.Password);

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Create Patient Profile
                var patient = new Patient
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = user.Id,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    DateOfBirth = model.DateOfBirth,
                    Gender = model.Gender,
                    Phone = model.Phone ?? string.Empty,
                    Address = model.Address ?? string.Empty,
                    BloodGroup = model.BloodGroup ?? string.Empty,
                    CreatedAt = DateTime.Now
                };

                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"New patient registered: {user.Email}");

                TempData["Success"] = "Registration successful! Please login.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                ModelState.AddModelError(string.Empty, "An error occurred during registration. Please try again.");
                return View(model);
            }
        }

        // LOGOUT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            HttpContext.Session.Clear();
            
            _logger.LogInformation($"User {userEmail} logged out.");
            
            TempData["Success"] = "You have been logged out successfully.";
            return RedirectToAction("Login");
        }

        // Helper method to redirect based on role
        private IActionResult RedirectToDashboard(string? role)
        {
            return role switch
            {
                "Admin" => RedirectToAction("AdminDashboard", "Home"),
                "Doctor" => RedirectToAction("DoctorDashboard", "Home"),
                "Patient" => RedirectToAction("PatientDashboard", "Home"),
                _ => RedirectToAction("Index", "Home"),
            };
        }
    }
}

