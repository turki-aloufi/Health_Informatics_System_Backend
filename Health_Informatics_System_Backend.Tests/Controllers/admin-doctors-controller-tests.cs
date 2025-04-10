using Health_Informatics_System_Backend.Controllers;
using Health_Informatics_System_Backend.Data;
using Health_Informatics_System_Backend.DTOs;
using Health_Informatics_System_Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Health_Informatics_System_Backend.Tests
{
    public class AdminDoctorsControllerTests
    {
        private readonly AppDbContext _context;
        private readonly AdminDoctorsController _controller;
        private readonly string _doctorIdPublic;

        public AdminDoctorsControllerTests()
        {
            // Set up in-memory database
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB per test class instance
                .Options;

            _context = new AppDbContext(options);

            // Seed data
            var doctorUser = new User
            {
                Name = "Doctor User",
                Email = "doctor@example.com",
                Role = UserRole.Doctor,
                Password = BCrypt.Net.BCrypt.HashPassword("doctor123"),
                DoB = new DateTime(1985, 5, 15),
                SSN = "222-22-2222",
                Gender = Gender.Female,
                PhoneNumber = "2345678901",
                Address = "Doctor Lane"
            };

            _context.Users.Add(doctorUser);
            _context.SaveChanges();

            var doctorProfile = new DoctorProfile
            {
                UserId = doctorUser.Id,
                Specialty = "Cardiology",
                LicenseNumber = "DOC123456",
                Clinic = "HeartCare Clinic"
            };

            var availability = new DoctorAvailability
            {
                DoctorId = doctorUser.Id,
                DayOfWeek = 1, // Monday
                StartTime = new TimeSpan(9, 0, 0),
                EndTime = new TimeSpan(17, 0, 0)
            };

            _context.DoctorProfiles.Add(doctorProfile);
            _context.DoctorAvailabilities.Add(availability);
            _context.SaveChanges();

            _doctorIdPublic = doctorUser.IdPublic;
            _controller = new AdminDoctorsController(_context);
        }

        #region GetAllDoctors Tests
        [Fact]
        public async Task GetAllDoctors_ReturnsAllDoctors()
        {
            // Act
            var result = await _controller.GetAllDoctors();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as dynamic;
            Assert.Equal("Retrieved doctors", response.msg);
            var doctors = response.data as IEnumerable<dynamic>;
            Assert.Single(doctors);
            var doctor = doctors.First();
            Assert.Equal("Doctor User", doctor.name);
            Assert.Equal("doctor@example.com", doctor.email);
            Assert.Equal("Cardiology", doctor.specialty);
        }
        #endregion

        #region GetDoctorById Tests
        [Fact]
        public async Task GetDoctorById_ExistingId_ReturnsDoctor()
        {
            // Act
            var result = await _controller.GetDoctorById(_doctorIdPublic);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as dynamic;
            Assert.Equal("Retrieved doctor", response.msg);
            var doctor = response.data;
            Assert.Equal(_doctorIdPublic, doctor.idPublic);
            Assert.Equal("Doctor User", doctor.name);
            Assert.Equal(1, doctor.availabilities.Count);
        }

        [Fact]
        public async Task GetDoctorById_NonExistingId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.GetDoctorById("non-existent-id");

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = notFoundResult.Value as dynamic;
            Assert.Equal("Doctor not found", response.msg);
            Assert.Null(response.data);
        }
        #endregion

        #region UpdateDoctor Tests
        [Fact]
        public async Task UpdateDoctor_ExistingId_UpdatesDoctor()
        {
            // Arrange
            var updateDto = new AdminUpdateDoctorDto
            {
                Name = "Updated Doctor",
                Email = "updated@example.com",
                DoB = new DateTime(1985, 5, 15),
                SSN = "222-22-2222",
                Gender = Gender.Female,
                PhoneNumber = "9876543210",
                Address = "Updated Lane",
                Specialty = "Neurology",
                LicenseNumber = "DOC654321",
                Clinic = "BrainCare Clinic",
                Availabilities = new List<DoctorAvailabilityDto>
                {
                    new DoctorAvailabilityDto { DayOfWeek = 2, StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(18, 0, 0) }
                }
            };

            // Act
            var result = await _controller.UpdateDoctor(_doctorIdPublic, updateDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as dynamic;
            Assert.Equal("Doctor profile updated successfully.", response.msg);

            var updatedUser = await _context.Users.FirstOrDefaultAsync(u => u.IdPublic == _doctorIdPublic);
            Assert.Equal("Updated Doctor", updatedUser.Name);
            Assert.Equal("updated@example.com", updatedUser.Email);

            var updatedProfile = await _context.DoctorProfiles.FirstOrDefaultAsync(dp => dp.UserId == updatedUser.Id);
            Assert.Equal("Neurology", updatedProfile.Specialty);

            var availabilities = await _context.DoctorAvailabilities.Where(a => a.DoctorId == updatedUser.Id).ToListAsync();
            Assert.Single(availabilities);
            Assert.Equal(2, availabilities[0].DayOfWeek);
        }

        [Fact]
        public async Task UpdateDoctor_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            var updateDto = new AdminUpdateDoctorDto { Name = "Test" /* minimal required fields */ };

            // Act
            var result = await _controller.UpdateDoctor("non-existent-id", updateDto);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Doctor profile not found.", notFoundResult.Value);
        }
        #endregion

        #region CreateDoctor Tests
        [Fact]
        public async Task CreateDoctor_ValidData_CreatesDoctor()
        {
            // Arrange
            var createDto = new AdminCreateDoctorDto
            {
                Name = "New Doctor",
                Email = "newdoctor@example.com",
                Password = "newpass123",
                DoB = new DateTime(1990, 1, 1),
                SSN = "333-33-3333",
                Gender = Gender.Male,
                PhoneNumber = "1234567890",
                Address = "New Street",
                Specialty = "Pediatrics",
                LicenseNumber = "DOC789012",
                Clinic = "Kids Clinic"
            };

            // Act
            var result = await _controller.CreateDoctor(createDto);

            // Assert
            var createdResult = Assert.IsType<CreatedResult>(result);
            var response = createdResult.Value as dynamic;
            Assert.Equal("Doctor created successfully", response.msg);
            var data = response.data;
            Assert.Equal("New Doctor", data.name);

            var newUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "newdoctor@example.com");
            Assert.NotNull(newUser);
            Assert.Equal(UserRole.Doctor, newUser.Role);
            Assert.True(BCrypt.Net.BCrypt.Verify("newpass123", newUser.Password));

            var newProfile = await _context.DoctorProfiles.FirstOrDefaultAsync(dp => dp.UserId == newUser.Id);
            Assert.Equal("Pediatrics", newProfile.Specialty);
        }

        [Fact]
        public async Task CreateDoctor_ExistingEmail_ReturnsBadRequest()
        {
            // Arrange
            var createDto = new AdminCreateDoctorDto
            {
                Name = "Duplicate Doctor",
                Email = "doctor@example.com", // Existing email
                Password = "pass123",
                DoB = new DateTime(1990, 1, 1),
                SSN = "444-44-4444",
                Gender = Gender.Male
            };

            // Act
            var result = await _controller.CreateDoctor(createDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var response = badRequestResult.Value as dynamic;
            Assert.Equal("Email already exists", response.msg);
        }
        #endregion

        #region DeleteDoctor Tests
        [Fact]
        public async Task DeleteDoctor_ExistingId_DeletesDoctor()
        {
            // Act
            var result = await _controller.DeleteDoctor(_doctorIdPublic);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value as dynamic;
            Assert.Equal("Doctor deleted successfully", response.msg);

            var deletedUser = await _context.Users.FirstOrDefaultAsync(u => u.IdPublic == _doctorIdPublic);
            Assert.Null(deletedUser);
        }

        [Fact]
        public async Task DeleteDoctor_NonExistingId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.DeleteDoctor("non-existent-id");

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = notFoundResult.Value as dynamic;
            Assert.Equal("Doctor not found", response.msg);
        }
        #endregion

        #region GenerateDoctorPdf Tests
        [Fact]
        public async Task GenerateDoctorPdf_ExistingId_ReturnsPdfFile()
        {
            // Act
            var result = await _controller.GenerateDoctorPdf(_doctorIdPublic);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/pdf", fileResult.ContentType);
            Assert.NotEmpty(fileResult.FileContents);
            Assert.Equal($"DoctorProfile_{_doctorIdPublic}.pdf", fileResult.FileDownloadName);
        }

        [Fact]
        public async Task GenerateDoctorPdf_NonExistingId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.GenerateDoctorPdf("non-existent-id");

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var response = notFoundResult.Value as dynamic;
            Assert.Equal("Doctor not found", response.msg);
        }
        #endregion
    }
}