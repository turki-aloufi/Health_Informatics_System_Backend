using Health_Informatics_System_Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Health_Informatics_System_Backend.Data
{
    public static class DbInitializer
    {
        public static void Seed(AppDbContext context)
        {
            context.Database.Migrate();

            if (context.Users.Any()) return; // Skip seeding if there are already users

            var adminUser = new User
            {
                Name = "Admin User",
                Email = "a@a.com",
                Role = UserRole.Admin,
                Password = BCrypt.Net.BCrypt.HashPassword("admin123"),
                DoB = new DateTime(1990, 1, 1),
                SSN = "111-11-1111",
                Gender = Gender.Male,
                PhoneNumber = "1234567890",
                Address = "Admin Street"
            };

            var doctorUser = new User
            {
                Name = "Doctor User",
                Email = "d@d.com",
                Role = UserRole.Doctor,
                Password = BCrypt.Net.BCrypt.HashPassword("doctor123"),
                DoB = new DateTime(1985, 5, 15),
                SSN = "222-22-2222",
                Gender = Gender.Female,
                PhoneNumber = "2345678901",
                Address = "Doctor Lane"
            };

            var patientUser = new User
            {
                Name = "Patient User",
                Email = "p@p.com",
                Role = UserRole.Patient,
                Password = BCrypt.Net.BCrypt.HashPassword("patient123"),
                DoB = new DateTime(2000, 10, 10),
                SSN = "333-33-3333",
                Gender = Gender.Male,
                PhoneNumber = "3456789012",
                Address = "Patient Ave"
            };

            context.Users.AddRange(adminUser, doctorUser, patientUser);
            context.SaveChanges();

            var doctorProfile = new DoctorProfile
            {
                UserId = doctorUser.Id,
                Specialty = "Cardiology",
                LicenseNumber = "DOC123456",
                Clinic = "HeartCare Clinic"
            };

            var patientProfile = new PatientProfile
            {
                UserId = patientUser.Id,
                MedicalHistory = "No major illnesses",
                InsuranceDetails = "HealthPlus Insurance - Policy #78910",
                EmergencyContact = "John Doe - 9876543210"
            };

            context.DoctorProfiles.Add(doctorProfile);
            context.PatientProfiles.Add(patientProfile);
            context.SaveChanges();
        }
    }
}
