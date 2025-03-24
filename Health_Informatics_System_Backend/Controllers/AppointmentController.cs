using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Health_Informatics_System_Backend.Data;
using Health_Informatics_System_Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
    }
}
