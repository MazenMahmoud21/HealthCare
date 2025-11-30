using System.Diagnostics;
using HealthcareSystem.Models;
using HealthcareSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HealthcareSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly HealthcareDbContext _context;

        public HomeController(ILogger<HomeController> logger, HealthcareDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            // If user is logged in, redirect to their dashboard
            var role = HttpContext.Session.GetString("Role");
            if (!string.IsNullOrEmpty(role))
            {
                return role switch
                {
                    "Admin" => RedirectToAction("AdminDashboard"),
                    "Doctor" => RedirectToAction("DoctorDashboard"),
                    "Patient" => RedirectToAction("PatientDashboard"),
                    _ => View()
                };
            }
            return View();
        }

        public IActionResult AdminDashboard()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.TotalPatients = _context.Patients.Count();
            ViewBag.TotalDoctors = _context.Doctors.Count();
            ViewBag.TotalAppointments = _context.Appointments.Count();
            ViewBag.PendingAppointments = _context.Appointments.Count(a => a.Status == "Scheduled");
            ViewBag.CompletedAppointments = _context.Appointments.Count(a => a.Status == "Completed");
            ViewBag.TotalMedicalRecords = _context.MedicalRecords.Count();
            
            ViewBag.RecentAppointments = _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .OrderByDescending(a => a.AppointmentDate)
                .Take(5)
                .ToList();

            return View();
        }

        public IActionResult DoctorDashboard()
        {
            var role = HttpContext.Session.GetString("Role");
            var userId = HttpContext.Session.GetString("UserId");
            
            if (role != "Doctor" || string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var doctor = _context.Doctors.FirstOrDefault(d => d.UserId == userId);
            if (doctor == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Doctor = doctor;
            ViewBag.DoctorName = $"Dr. {doctor.FirstName} {doctor.LastName}";
            
            var today = DateTime.Today;
            ViewBag.TodayAppointments = _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctor.Id && a.AppointmentDate.Date == today)
                .OrderBy(a => a.AppointmentDate)
                .ToList();

            ViewBag.UpcomingAppointments = _context.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctor.Id && a.AppointmentDate > DateTime.Now && a.Status == "Scheduled")
                .OrderBy(a => a.AppointmentDate)
                .Take(10)
                .ToList();

            ViewBag.TotalPatientsSeen = _context.Appointments
                .Where(a => a.DoctorId == doctor.Id && a.Status == "Completed")
                .Select(a => a.PatientId)
                .Distinct()
                .Count();

            ViewBag.TotalAppointments = _context.Appointments.Count(a => a.DoctorId == doctor.Id);
            ViewBag.CompletedAppointments = _context.Appointments.Count(a => a.DoctorId == doctor.Id && a.Status == "Completed");
            ViewBag.PendingAppointments = _context.Appointments.Count(a => a.DoctorId == doctor.Id && a.Status == "Scheduled");

            return View();
        }

        public IActionResult PatientDashboard()
        {
            var role = HttpContext.Session.GetString("Role");
            var userId = HttpContext.Session.GetString("UserId");
            
            if (role != "Patient" || string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var patient = _context.Patients.FirstOrDefault(p => p.UserId == userId);
            if (patient == null)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.Patient = patient;
            ViewBag.PatientName = $"{patient.FirstName} {patient.LastName}";

            ViewBag.UpcomingAppointments = _context.Appointments
                .Include(a => a.Doctor)
                .Where(a => a.PatientId == patient.Id && a.AppointmentDate >= DateTime.Now && a.Status == "Scheduled")
                .OrderBy(a => a.AppointmentDate)
                .Take(5)
                .ToList();

            ViewBag.PastAppointments = _context.Appointments
                .Include(a => a.Doctor)
                .Where(a => a.PatientId == patient.Id && (a.Status == "Completed" || a.AppointmentDate < DateTime.Now))
                .OrderByDescending(a => a.AppointmentDate)
                .Take(5)
                .ToList();

            ViewBag.MedicalRecords = _context.MedicalRecords
                .Include(m => m.Doctor)
                .Where(m => m.PatientId == patient.Id)
                .OrderByDescending(m => m.VisitDate)
                .Take(5)
                .ToList();

            ViewBag.Prescriptions = _context.Prescriptions
                .Include(p => p.Doctor)
                .Where(p => p.PatientId == patient.Id)
                .OrderByDescending(p => p.PrescriptionDate)
                .Take(5)
                .ToList();

            ViewBag.TotalAppointments = _context.Appointments.Count(a => a.PatientId == patient.Id);
            ViewBag.TotalMedicalRecords = _context.MedicalRecords.Count(m => m.PatientId == patient.Id);
            ViewBag.TotalPrescriptions = _context.Prescriptions.Count(p => p.PatientId == patient.Id);

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
