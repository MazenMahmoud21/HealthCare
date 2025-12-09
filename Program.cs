using HealthcareSystem.Data;
using HealthcareSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add DbContext with SQLite
builder.Services.AddDbContext<HealthcareDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));


// Add Password Hasher
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// Add session if using HttpContext.Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Seed initial data
Task.Run(async () =>
{
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<HealthcareDbContext>();
            var hasher = services.GetRequiredService<IPasswordHasher<User>>();
            
            // Ensure the database is created
            await context.Database.EnsureCreatedAsync();
            
            // Seed Admin user if not exists
            if (!context.Users.Any(u => u.Role == "Admin"))
            {
                var adminUser = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = "admin@healthcare.com",
                    Role = "Admin",
                    CreatedAt = DateTime.Now
                };
                adminUser.Password = hasher.HashPassword(adminUser, "Admin@123");
                context.Users.Add(adminUser);
                await context.SaveChangesAsync();
            }
            
            // Seed sample doctor if not exists
            if (!context.Doctors.Any())
            {
                var doctorUser = new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Email = "doctor@healthcare.com",
                    Role = "Doctor",
                    CreatedAt = DateTime.Now
                };
                doctorUser.Password = hasher.HashPassword(doctorUser, "Doctor@123");
                context.Users.Add(doctorUser);
                await context.SaveChangesAsync();
                
                var doctor = new Doctor
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = doctorUser.Id,
                    FirstName = "Doctor",
                    LastName = "1",
                    Specialization = "General Practice",
                    Phone = "+20 100 000 0001",
                    Email = "doctor@healthcare.com",
                    CreatedAt = DateTime.Now
                };
                context.Doctors.Add(doctor);
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while seeding the database. This is normal on first run if database doesn't exist yet.");
        }
    }
}).Wait();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // Use session middleware
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
