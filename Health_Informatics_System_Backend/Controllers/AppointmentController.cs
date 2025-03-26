using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Health_Informatics_System_Backend.Data;
using Health_Informatics_System_Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Health_Informatics_System_Backend.DTOs.Health_Informatics_System_Backend.Dtos;

namespace Health_Informatics_System_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public AppointmentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Appointments/me
        [HttpGet("me")]
        public async Task<IActionResult> GetMyAppointments()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            int userId = int.Parse(userIdClaim);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role == "Patient")
            {
                var appointments = await _context.Appointments
                    .Where(a => a.PatientId == userId)
                    .ToListAsync();
                return Ok(new {msg = "appointments",
                data = appointments});
            }
            else if (role == "Doctor")
            {
                var appointments = await _context.Appointments
                    .Where(a => a.DoctorId == userId)
                    .ToListAsync();
                return Ok(new {msg = "appointments",
                data = appointments});
            }
            return Forbid();
        }
        [HttpPost]
        public async Task<IActionResult> CreateAppointment([FromBody] AppointmentCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var appointment = new Appointment
            {
                PatientId = dto.PatientId,
                DoctorId = dto.DoctorId,
                AppointmentDateTime = dto.AppointmentDateTime,
                Status = dto.Status,
                Notes = dto.Notes
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMyAppointments), new { id = appointment.AppointmentId }, appointment);
        }




    }
}
