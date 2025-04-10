using Health_Informatics_System_Backend.Data;
using Health_Informatics_System_Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using System.Text.Json;
using Health_Informatics_System_Backend.DTOs;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Font.Constants;
using iText.Kernel.Font;

namespace Health_Informatics_System_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminDoctorsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public AdminDoctorsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("get-all-doctors")]
        public async Task<ActionResult> GetAllDoctors()
        {
            var doctors = await _context.DoctorProfiles
                .Include(d => d.User)
                .Include(d => d.Availabilities)
                .Select(d => new
                {
                    idPublic = d.User.IdPublic,
                    name = d.User.Name,
                    email = d.User.Email,
                    role = d.User.Role,
                    doB = d.User.DoB,
                    ssn = d.User.SSN,
                    gender = d.User.Gender,
                    phoneNumber = d.User.PhoneNumber,
                    address = d.User.Address,
                    specialty = d.Specialty,
                    licenseNumber = d.LicenseNumber,
                    clinic = d.Clinic,
                    availabilities = d.Availabilities.Select(a => new
                    {
                        availabilityIdPublic = a.AvailabilityIdPublic,
                        dayOfWeek = a.DayOfWeek,
                        startTime = a.StartTime,
                        endTime = a.EndTime
                    }).ToList()
                })
                .ToListAsync();

            var response = new
            {
                msg = "Retrieved doctors",
                data = doctors
            };
            return Ok(response);
        }

        // GET: api/AdminDoctors/get-doctor-by-id/{id}
        [HttpGet("get-doctor-by-id/{id}")]
        public async Task<ActionResult> GetDoctorById(string id)
        {
            var doctor = await _context.DoctorProfiles
                .Include(d => d.User)
                .Include(d => d.Availabilities)
                .Select(d => new
                {
                    idPublic = d.User.IdPublic,
                    name = d.User.Name,
                    email = d.User.Email,
                    role = d.User.Role,
                    doB = d.User.DoB,
                    ssn = d.User.SSN,
                    gender = d.User.Gender,
                    phoneNumber = d.User.PhoneNumber,
                    address = d.User.Address,
                    specialty = d.Specialty,
                    licenseNumber = d.LicenseNumber,
                    clinic = d.Clinic,
                    availabilities = d.Availabilities.Select(a => new
                    {
                        availabilityIdPublic = a.AvailabilityIdPublic,
                        dayOfWeek = a.DayOfWeek,
                        startTime = a.StartTime,
                        endTime = a.EndTime
                    }).ToList()
                })
                .FirstOrDefaultAsync(d => d.idPublic == id);

            if (doctor == null)
            {
                return NotFound(new
                {
                    msg = "Doctor not found",
                    data = (object)null
                });
            }

            return Ok(new
            {
                msg = "Retrieved doctor",
                data = doctor
            });
        }

        // PUT: api/AdminDoctors/update-doctor/{id}
        [HttpPut("update-doctor/{id}")]
        public async Task<ActionResult> UpdateDoctor(string id, [FromBody] AdminUpdateDoctorDto doctorDto)
        {
            if (doctorDto == null)
            {
                return BadRequest("Invalid request data.");
            }

            var doctorProfile = await _context.DoctorProfiles
                .Include(d => d.User)
                .Include(d => d.Availabilities)
                .FirstOrDefaultAsync(d => d.User.IdPublic == id);

            if (doctorProfile == null)
            {
                return NotFound("Doctor profile not found.");
            }

            // Update User properties
            doctorProfile.User.Name = doctorDto.Name;
            doctorProfile.User.PhoneNumber = doctorDto.PhoneNumber;
            doctorProfile.User.Address = doctorDto.Address;
            doctorProfile.User.DoB = doctorDto.DoB;
            doctorProfile.User.Email = doctorDto.Email;
            doctorProfile.User.Gender = doctorDto.Gender;
            doctorProfile.User.SSN = doctorDto.SSN;
            
            // Update Doctor-specific properties
            doctorProfile.Specialty = doctorDto.Specialty;
            doctorProfile.LicenseNumber = doctorDto.LicenseNumber;
            doctorProfile.Clinic = doctorDto.Clinic;

            // Update availabilities if provided
            if (doctorDto.Availabilities != null && doctorDto.Availabilities.Any())
            {
                // Remove existing availabilities
                foreach (var availability in doctorProfile.Availabilities.ToList())
                {
                    _context.Remove(availability);
                }

                // Add new availabilities
                foreach (var availabilityDto in doctorDto.Availabilities)
                {
                    var availability = new DoctorAvailability
                    {
                        DoctorId = doctorProfile.UserId,
                        DayOfWeek = availabilityDto.DayOfWeek,
                        StartTime = availabilityDto.StartTime,
                        EndTime = availabilityDto.EndTime
                    };
                    _context.DoctorAvailabilities.Add(availability);
                }
            }

            _context.DoctorProfiles.Update(doctorProfile);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                msg = "Doctor profile updated successfully.",
                data = doctorDto
            });
        }

        // POST: api/AdminDoctors/create-doctor
        [HttpPost("create-doctor")]
        public async Task<ActionResult> CreateDoctor([FromBody] AdminCreateDoctorDto doctorDto)
        {
            if (doctorDto == null)
            {
                return BadRequest("Invalid request data.");
            }

            // Check if email already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == doctorDto.Email);
            if (existingUser != null)
            {
                return BadRequest(new
                {
                    msg = "Email already exists",
                    data = (object)null
                });
            }

            // Create new user with hashed password
            var user = new User
            {
                Name = doctorDto.Name,
                Email = doctorDto.Email,
                Password = HashPassword(doctorDto.Password), // Hash the password
                Role = UserRole.Doctor,
                DoB = doctorDto.DoB,
                SSN = doctorDto.SSN,
                Gender = doctorDto.Gender,
                PhoneNumber = doctorDto.PhoneNumber,
                Address = doctorDto.Address
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create doctor profile
            var doctorProfile = new DoctorProfile
            {
                UserId = user.Id,
                Specialty = doctorDto.Specialty,
                LicenseNumber = doctorDto.LicenseNumber,
                Clinic = doctorDto.Clinic
            };

            _context.DoctorProfiles.Add(doctorProfile);
            await _context.SaveChangesAsync();

            // Add availabilities if provided
            if (doctorDto.Availabilities != null && doctorDto.Availabilities.Any())
            {
                foreach (var availabilityDto in doctorDto.Availabilities)
                {
                    var availability = new DoctorAvailability
                    {
                        DoctorId = doctorProfile.UserId,
                        DayOfWeek = availabilityDto.DayOfWeek,
                        StartTime = availabilityDto.StartTime,
                        EndTime = availabilityDto.EndTime
                    };
                    _context.DoctorAvailabilities.Add(availability);
                }
                await _context.SaveChangesAsync();
            }

            return Created(string.Empty, new
            {
                msg = "Doctor created successfully",
                data = new
                {
                    idPublic = user.IdPublic,
                    name = user.Name,
                    email = user.Email,
                    role = user.Role,
                    specialty = doctorProfile.Specialty,
                    licenseNumber = doctorProfile.LicenseNumber,
                    clinic = doctorProfile.Clinic
                }
            });
        }

        // DELETE: api/AdminDoctors/delete-doctor/{id}
        [HttpDelete("delete-doctor/{id}")]
        public async Task<ActionResult> DeleteDoctor(string id)
        {
            var user = await _context.Users.Where(u => u.IdPublic == id).FirstOrDefaultAsync();
            if (user == null)
            {
                return NotFound(new
                {
                    msg = "Doctor not found",
                    data = (object)null
                });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                msg = "Doctor deleted successfully",
                data = (object)null
            });
        }

        [HttpGet("generate-doctor-pdf/{id}")]
        public async Task<IActionResult> GenerateDoctorPdf(string id)
        {
            var doctor = await _context.DoctorProfiles
                .Include(d => d.User)
                .Include(d => d.Availabilities)
                .Select(d => new
                {
                    idPublic = d.User.IdPublic,
                    name = d.User.Name,
                    email = d.User.Email,
                    role = d.User.Role,
                    doB = d.User.DoB,
                    ssn = d.User.SSN,
                    gender = d.User.Gender,
                    phoneNumber = d.User.PhoneNumber,
                    address = d.User.Address,
                    specialty = d.Specialty,
                    licenseNumber = d.LicenseNumber,
                    clinic = d.Clinic,
                    availabilities = d.Availabilities.Select(a => new
                    {
                        dayOfWeek = a.DayOfWeek,
                        startTime = a.StartTime,
                        endTime = a.EndTime
                    }).ToList()
                })
                .FirstOrDefaultAsync(d => d.idPublic == id);

            if (doctor == null)
            {
                return NotFound(new
                {
                    msg = "Doctor not found",
                    data = (object)null
                });
            }

            using (var memoryStream = new MemoryStream())
            {
                var writer = new PdfWriter(memoryStream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                // Add title with bold formatting using font
                document.Add(new Paragraph("Doctor Profile")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(20)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)));

                // Add doctor information
                document.Add(new Paragraph($"Name: {doctor.name}"));
                document.Add(new Paragraph($"Email: {doctor.email}"));
                document.Add(new Paragraph($"Role: {doctor.role}"));
                document.Add(new Paragraph($"Date of Birth: {doctor.doB:yyyy-MM-dd}"));
                document.Add(new Paragraph($"SSN: {doctor.ssn}"));
                document.Add(new Paragraph($"Gender: {doctor.gender}"));
                document.Add(new Paragraph($"Phone: {doctor.phoneNumber}"));
                document.Add(new Paragraph($"Address: {doctor.address}"));
                document.Add(new Paragraph($"Specialty: {doctor.specialty}"));
                document.Add(new Paragraph($"License Number: {doctor.licenseNumber}"));
                document.Add(new Paragraph($"Clinic: {doctor.clinic}"));

                // Add availabilities section with bold formatting using font
                document.Add(new Paragraph("\nAvailabilities:")
                    .SetFontSize(14)
                    .SetFont(PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD)));

                if (doctor.availabilities.Any())
                {
                    Table table = new Table(3);
                    table.AddHeaderCell("Day of Week");
                    table.AddHeaderCell("Start Time");
                    table.AddHeaderCell("End Time");

                    foreach (var availability in doctor.availabilities)
                    {
                        table.AddCell(availability.dayOfWeek.ToString());
                        table.AddCell(availability.startTime.ToString(@"hh\:mm"));
                        table.AddCell(availability.endTime.ToString(@"hh\:mm"));
                    }
                    document.Add(table);
                }
                else
                {
                    document.Add(new Paragraph("No availability scheduled"));
                }

                // Add generation timestamp
                document.Add(new Paragraph($"\nGenerated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                    .SetFontSize(10)
                    .SetTextAlignment(TextAlignment.RIGHT));

                document.Close();

                var pdfBytes = memoryStream.ToArray();
                return File(pdfBytes, "application/pdf", $"DoctorProfile_{doctor.idPublic}.pdf");
            }
        }
        // Helper method to hash passwords
        private string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password cannot be null or empty.");
            }
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}