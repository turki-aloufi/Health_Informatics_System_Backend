using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Health_Informatics_System_Backend.Data;
using Health_Informatics_System_Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Health_Informatics_System_Backend.Controllers
{
    [Authorize(Roles = "Doctor")]
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorProfileController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DoctorProfileController(AppDbContext context)
        {
            _context = context;
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
                .OrderBy(a => a.AppointmentDateTime)
                .ToListAsync();

            return Ok(upcomingAppointments);
        }

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
                .OrderByDescending(a => a.AppointmentDateTime)
                .ToListAsync();

            return Ok(pastAppointments);
        }

        // GET: api/DoctorProfile/me
        [HttpGet("me")]
        public async Task<IActionResult> GetMyDoctorProfile()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("User ID not found in token.");

            int userId = int.Parse(userIdClaim);

            var doctorProfile = await _context.DoctorProfiles
                .Include(d => d.User) // Optional: get name/email/etc.
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctorProfile == null)
                return NotFound("Doctor profile not found.");

            // Optional: create a DTO to return a clean result
            return Ok(new
            {
                doctorProfile.UserId,
                doctorProfile.User.IdPublic,
                Name = doctorProfile.User.Name,
                Email = doctorProfile.User.Email,
                doctorProfile.Specialty,
                doctorProfile.Clinic,
                doctorProfile.LicenseNumber
            });
        }
        // Change patient notes
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





    }
}
