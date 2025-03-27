using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Health_Informatics_System_Backend.Data;
using Health_Informatics_System_Backend.Models;
using Health_Informatics_System_Backend.DTOs.Health_Informatics_System_Backend.Dtos;
using Health_Informatics_System_Backend.Services;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Health_Informatics_System_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly AppointmentService _appointmentService;

        public AppointmentsController(AppDbContext context, AppointmentService appointmentService)
        {
            _context = context;
            _appointmentService = appointmentService;
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
                return Ok(new { msg = "appointments", data = appointments });
            }
            else if (role == "Doctor")
            {
                var appointments = await _context.Appointments
                    .Where(a => a.DoctorId == userId)
                    .ToListAsync();
                return Ok(new { msg = "appointments", data = appointments });
            }
            return Forbid();
        }

        // POST: api/Appointments
        [HttpPost]
        public async Task<IActionResult> CreateAppointment([FromBody] AppointmentCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var appointment = await _appointmentService.CreateAppointment(
                    dto.PatientId,
                    dto.DoctorId,
                    dto.AppointmentDateTime,
                    dto.Notes
                );
                return CreatedAtAction(nameof(GetMyAppointments), new { id = appointment.AppointmentId }, appointment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        
        [HttpGet("available-slots/{doctorId}")]
        public async Task<IActionResult> GetAvailableTimeSlots(
            [FromRoute] int doctorId, 
            [FromQuery] DateTime date)
        {
            var slots = await _appointmentService.GetAvailableTimeSlots(doctorId, date);
            return Ok(slots);
        }
    }
}
