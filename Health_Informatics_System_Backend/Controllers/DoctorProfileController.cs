using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Health_Informatics_System_Backend.Data;
using Microsoft.EntityFrameworkCore;
using Health_Informatics_System_Backend.Models;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Text;

namespace Health_Informatics_System_Backend.Controllers
{
    [Authorize(Roles = "Doctor")]
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorProfileController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IDistributedCache _cache;
        private readonly DistributedCacheEntryOptions _cacheOptions =
            new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(30));
        public DoctorProfileController(AppDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET: api/DoctorProfile/appointments/upcoming
        [HttpGet("appointments/upcoming")]
        public async Task<IActionResult> GetUpcomingAppointments()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("User ID not found in token.");

            int doctorId = int.Parse(userIdClaim);

            var upcomingAppointments = await _context.Appointments
    .Where(a => a.DoctorId == doctorId &&
                a.AppointmentDateTime >= DateTime.Now &&
                a.Status == AppointmentStatus.Scheduled)
    .Include(a => a.PatientProfile)
    .ThenInclude(p => p.User)
    .OrderBy(a => a.AppointmentDateTime)
    .Select(a => new
    {
        a.AppointmentId,
        a.AppointmentIdPublic,
        a.PatientId,
        a.DoctorId,
        a.AppointmentDateTime,
        a.Status,
        a.Notes,
        PatientName = a.PatientProfile.User.Name,
        PatientEmail = a.PatientProfile.User.Email,
        PatientPhone = a.PatientProfile.User.PhoneNumber,
        PatientGender = a.PatientProfile.User.Gender
    })
    .ToListAsync();

            return Ok(upcomingAppointments);
        }

        // GET: api/DoctorProfile/appointments/past
       // GET: api/DoctorProfile/appointments/past
[HttpGet("appointments/past")]
public async Task<IActionResult> GetPastAppointments()
{
    var userIdClaim = User.FindFirst("UserId")?.Value;
    if (string.IsNullOrEmpty(userIdClaim))
        return Unauthorized("User ID not found in token.");

    int doctorId = int.Parse(userIdClaim);

    var pastAppointments = await _context.Appointments
        .Where(a => a.DoctorId == doctorId &&
                    a.AppointmentDateTime < DateTime.Now &&
                    a.Status == AppointmentStatus.Completed)
        .Include(a => a.PatientProfile)
        .ThenInclude(p => p.User)
        .OrderByDescending(a => a.AppointmentDateTime)
        .Select(a => new
        {
            a.AppointmentId,
            a.AppointmentIdPublic,
            a.PatientId,
            a.DoctorId,
            a.AppointmentDateTime,
            a.Status,
            a.Notes,
            PatientName = a.PatientProfile.User.Name,
            PatientEmail = a.PatientProfile.User.Email,
            PatientPhone = a.PatientProfile.User.PhoneNumber,
            PatientGender = a.PatientProfile.User.Gender
        })
        .ToListAsync();

    return Ok(pastAppointments);
}

        // GET: api/DoctorProfile/me
        //HGETALL HealthInfSys_DoctorProfile_8
        [HttpGet("me")]
        public async Task<IActionResult> GetMyDoctorProfile()
        {
             var userIdClaim = User.FindFirst("UserId")?.Value;
             if (string.IsNullOrEmpty(userIdClaim))
             return Unauthorized("User ID not found in token.");

             int userId = int.Parse(userIdClaim);

             // Construct a cache key (no need to worry about prefix here)
            var cacheKey = $"DoctorProfile_{userId}";

             try
                {
                // Try to get from cache
                  string cachedJson = await _cache.GetStringAsync(cacheKey);
        
                if (!string.IsNullOrEmpty(cachedJson))
                 {
                 Console.WriteLine($"Cache hit for {cacheKey}");
                 var cachedProfile = JsonSerializer.Deserialize<DoctorProfileDto>(cachedJson);
                    return Ok(cachedProfile);
                }
        
        Console.WriteLine($"Cache miss for {cacheKey}, fetching from database");
        
        // If not in cache, get from database
        var doctorProfile = await _context.DoctorProfiles
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.UserId == userId);

        if (doctorProfile == null)
            return NotFound("Doctor profile not found.");

        // Create response DTO
        var profileDto = new DoctorProfileDto
        {
            UserId = doctorProfile.UserId,
            IdPublic = doctorProfile.User.IdPublic,
            Name = doctorProfile.User.Name,
            Email = doctorProfile.User.Email,
            Specialty = doctorProfile.Specialty,
            Clinic = doctorProfile.Clinic,
            LicenseNumber = doctorProfile.LicenseNumber
        };

        // Store in cache with debug logging
        var serializedData = JsonSerializer.Serialize(profileDto);
        Console.WriteLine($"Caching data for {cacheKey}: {serializedData}");
        await _cache.SetStringAsync(cacheKey, serializedData, _cacheOptions);
        Console.WriteLine("Cache set completed");

        return Ok(profileDto);
    }
    catch (Exception ex)
    {
        // Log the exception with details
        Console.WriteLine($"Redis caching error: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        
        // Fallback to database
        var doctorProfile = await _context.DoctorProfiles
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.UserId == userId);

        if (doctorProfile == null)
            return NotFound("Doctor profile not found.");

        return Ok(new DoctorProfileDto
        {
            UserId = doctorProfile.UserId,
            IdPublic = doctorProfile.User.IdPublic,
            Name = doctorProfile.User.Name,
            Email = doctorProfile.User.Email,
            Specialty = doctorProfile.Specialty,
            Clinic = doctorProfile.Clinic,
            LicenseNumber = doctorProfile.LicenseNumber
        });
    }
}
        // PUT: api/DoctorProfile/appointments/{appointmentId}/notes
        [HttpPut("appointments/public/{appointmentPublicId}/notes")]
        public async Task<IActionResult> UpdateAppointmentNotes(string appointmentPublicId, [FromBody] string notes)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("User ID not found in token.");

            int doctorId = int.Parse(userIdClaim);

            
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentIdPublic == appointmentPublicId && a.DoctorId == doctorId);

            if (appointment == null)
                return NotFound("Appointment not found or you are not authorized to modify it.");

            appointment.Notes = notes;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Notes updated successfully.", appointment.Notes });
        }
        // Get the patient notes from other appointments
        // GET: api/DoctorProfile/appointments/{appointmentId}/patient-notes

        [HttpGet("appointments/public/{appointmentIdPublic}/patient-notes")]
        public async Task<IActionResult> GetPatientPastNotesFromAppointment(string appointmentIdPublic)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("User ID not found in token.");

            int doctorId = int.Parse(userIdClaim);

            var currentAppointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentIdPublic == appointmentIdPublic && a.DoctorId == doctorId);

            if (currentAppointment == null)
                return Forbid("You are not authorized to view this appointment or its patient's notes.");

            int patientId = currentAppointment.PatientId;

            var pastNotes = await _context.Appointments
                .Where(a => a.PatientId == patientId &&
                            a.DoctorId == doctorId &&
                            a.Status == AppointmentStatus.Completed &&
                            a.AppointmentDateTime < DateTime.Now &&
                            !string.IsNullOrEmpty(a.Notes))
                .OrderByDescending(a => a.AppointmentDateTime)
                .Select(a => new
                {
                    a.AppointmentId,
                    a.AppointmentDateTime,
                    a.Notes
                })
                .ToListAsync();

            return Ok(new
            {
                appointmentIdPublic,
                patientId,
                notesHistory = pastNotes
            });
        }
            private class DoctorProfileDto
        {
            public int UserId { get; set; }
            public string IdPublic { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string Specialty { get; set; }
            public string Clinic { get; set; }
            public string LicenseNumber { get; set; }
        }
    }
}
