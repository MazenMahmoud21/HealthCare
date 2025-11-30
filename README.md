# 🏥 Healthcare Management System

A modern, full-featured healthcare management system built with ASP.NET Core MVC. This application provides a comprehensive solution for managing patients, doctors, appointments, medical records, and prescriptions.

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?style=flat-square)
![Bootstrap](https://img.shields.io/badge/Bootstrap-5.3-7952B3?style=flat-square&logo=bootstrap&logoColor=white)
![License](https://img.shields.io/badge/License-MIT-green?style=flat-square)

## ✨ Features

### 👥 Multi-Role Authentication
- **Admin** - Full system access and management
- **Doctor** - Patient management, appointments, prescriptions
- **Patient** - Book appointments, view records and prescriptions

### 📋 Core Modules

| Module | Description |
|--------|-------------|
| **Patient Management** | Register patients, manage profiles, track medical history |
| **Doctor Management** | Add doctors, specializations, consultation fees |
| **Appointment System** | Book, reschedule, cancel appointments with calendar view |
| **Medical Records** | Create and manage patient medical records |
| **Prescriptions** | Digital prescriptions with print support |

### 🎨 Modern UI/UX
- Clean, minimal, and professional design
- Responsive dashboard for all user roles
- Real-time statistics and data visualization
- Mobile-friendly interface

## 🛠️ Tech Stack

- **Backend:** ASP.NET Core 8.0 MVC
- **Database:** Entity Framework Core with SQL Server
- **Frontend:** Bootstrap 5, Font Awesome, Inter Font
- **Authentication:** Session-based authentication

## 🚀 Getting Started

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/sql-server) (or SQL Server Express/LocalDB)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/MazenMahmoud21/HealthCare.git
   cd HealthCare
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Update database connection string**
   
   Edit `appsettings.json` with your SQL Server connection string:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER;Database=HealthcareDB;Trusted_Connection=True;"
     }
   }
   ```

4. **Apply database migrations**
   ```bash
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Open in browser**
   ```
   http://localhost:5049
   ```

## 📸 Screenshots

### Dashboard
- Admin Dashboard with system statistics
- Doctor Dashboard with appointment schedule
- Patient Dashboard with health overview

### Key Features
- Appointment booking with doctor selection
- Medical records management
- Digital prescription system

## 📁 Project Structure

```
HealthCare/
├── Controllers/          # MVC Controllers
│   ├── AccountController.cs
│   ├── AppointmentController.cs
│   ├── DoctorController.cs
│   ├── PatientController.cs
│   └── ...
├── Models/               # Data Models & DTOs
│   ├── Patient.cs
│   ├── Doctor.cs
│   ├── Appointment.cs
│   └── DTOs/
├── Views/                # Razor Views
│   ├── Account/
│   ├── Appointment/
│   ├── Doctor/
│   ├── Patient/
│   └── Shared/
├── Data/                 # Database Context
├── wwwroot/              # Static Files (CSS, JS)
└── Program.cs            # Application Entry Point
```

## 🔐 Default Roles

| Role | Access Level |
|------|--------------|
| Admin | Full access - manage all users, doctors, patients, appointments |
| Doctor | View patients, manage appointments, create prescriptions & records |
| Patient | Book appointments, view own records and prescriptions |

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👨‍💻 Author

**Mazen Mahmoud**

- GitHub: [@MazenMahmoud21](https://github.com/MazenMahmoud21)

---

<p align="center">
  Made with ❤️ in Egypt 🇪🇬
</p>
