using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Health_Informatics_System_Backend.Models;
using Health_Informatics_System_Backend.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Health_Informatics_System_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminAppointmentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminAppointmentsController(AppDbContext context)
        {
            _context = context;
        }

        // DTO for returning appointment data with names
        public class AppointmentResponseDto
        {
            public string AppointmentIdPublic { get; set; }
            public int PatientId { get; set; }
            public string PatientName { get; set; }
            public int DoctorId { get; set; }
            public string DoctorName { get; set; }
            public DateTime AppointmentDateTime { get; set; }
            public string Status { get; set; }
            public string Notes { get; set; }
        }

        // DTO for creating/updating appointments (renamed to avoid conflict)
        public class AdminAppointmentCreateDto
        {
            public int PatientId { get; set; }
            public int DoctorId { get; set; }
            public DateTime AppointmentDateTime { get; set; }
            public string Status { get; set; } // String to match frontend
            public string Notes { get; set; }
        }

        // GET: api/AdminAppointments/get-all-appointments
        [HttpGet("get-all-appointments")]
        public async Task<ActionResult<IEnumerable<AppointmentResponseDto>>> GetAppointments()
        {
            try
            {
                var appointments = await _context.Appointments
                    .Include(a => a.PatientProfile)
                    .ThenInclude(p => p.User)
                    .Include(a => a.DoctorProfile)
                    .ThenInclude(d => d.User)
                    .ToListAsync();

                var result = appointments.Select(a => new AppointmentResponseDto
                {
                    AppointmentIdPublic = a.AppointmentIdPublic,
                    PatientId = a.PatientId,
                    PatientName = a.PatientProfile?.User?.Name ?? "Unknown",
                    DoctorId = a.DoctorId,
                    DoctorName = a.DoctorProfile?.User?.Name ?? "Unknown",
                    AppointmentDateTime = a.AppointmentDateTime,
                    Status = a.Status.ToString(),
                    Notes = a.Notes ?? "None"
                }).ToList();

                return Ok(new { msg = "Appointments retrieved successfully", data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { msg = "Error retrieving appointments", error = ex.Message });
            }
        }

        // GET: api/AdminAppointments/get-appointment/{appointmentIdPublic}
        [HttpGet("get-appointment/{appointmentIdPublic}")]
        public async Task<ActionResult<AppointmentResponseDto>> GetAppointment(string appointmentIdPublic)
        {
            try
            {
                var appointment = await _context.Appointments
                    .Include(a => a.PatientProfile)
                    .ThenInclude(p => p.User)
                    .Include(a => a.DoctorProfile)
                    .ThenInclude(d => d.User)
                    .FirstOrDefaultAsync(a => a.AppointmentIdPublic == appointmentIdPublic);

                if (appointment == null)
                {
                    return NotFound(new { msg = "Appointment not found" });
                }

                var result = new AppointmentResponseDto
                {
                    AppointmentIdPublic = appointment.AppointmentIdPublic,
                    PatientId = appointment.PatientId,
                    PatientName = appointment.PatientProfile?.User?.Name ?? "Unknown",
                    DoctorId = appointment.DoctorId,
                    DoctorName = appointment.DoctorProfile?.User?.Name ?? "Unknown",
                    AppointmentDateTime = appointment.AppointmentDateTime,
                    Status = appointment.Status.ToString(),
                    Notes = appointment.Notes ?? "None"
                };

                return Ok(new { msg = "Appointment retrieved successfully", data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { msg = "Error retrieving appointment", error = ex.Message });
            }
        }

        // POST: api/AdminAppointments/create-appointment
        [HttpPost("create-appointment")]
        public async Task<ActionResult<Appointment>> CreateAppointment([FromBody] Appointment appointment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { msg = "Invalid appointment data", errors = ModelState });
            }

            try
            {
                var patientExists = await _context.PatientProfiles.AnyAsync(p => p.UserId == appointment.PatientId);
                var doctorExists = await _context.DoctorProfiles.AnyAsync(d => d.UserId == appointment.DoctorId);

                if (!patientExists || !doctorExists)
                {
                    return BadRequest(new { msg = "Invalid PatientId or DoctorId" });
                }

                appointment.AppointmentIdPublic = Guid.NewGuid().ToString();
                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetAppointment), new { appointmentIdPublic = appointment.AppointmentIdPublic },
                    new { msg = "Appointment created successfully", data = appointment });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { msg = "Error creating appointment", error = ex.Message });
            }
        }

        // PUT: api/AdminAppointments/update-appointment/{appointmentIdPublic}
        [HttpPut("update-appointment/{appointmentIdPublic}")]
        public async Task<IActionResult> UpdateAppointment(string appointmentIdPublic, [FromBody] AdminAppointmentCreateDto updatedAppointment)
        {
            if (!ModelState.IsValid || updatedAppointment == null)
            {
                return BadRequest(new { msg = "Invalid appointment data", errors = ModelState });
            }

            try
            {
                var appointment = await _context.Appointments
                    .FirstOrDefaultAsync(a => a.AppointmentIdPublic == appointmentIdPublic);

                if (appointment == null)
                {
                    return NotFound(new { msg = "Appointment not found" });
                }

                appointment.PatientId = updatedAppointment.PatientId;
                appointment.DoctorId = updatedAppointment.DoctorId;
                appointment.AppointmentDateTime = updatedAppointment.AppointmentDateTime;
                if (!Enum.TryParse<AppointmentStatus>(updatedAppointment.Status, true, out var status))
                {
                    return BadRequest(new { msg = "Invalid Status value" });
                }
                appointment.Status = status;
                appointment.Notes = updatedAppointment.Notes;

                var patientExists = await _context.PatientProfiles.AnyAsync(p => p.UserId == appointment.PatientId);
                var doctorExists = await _context.DoctorProfiles.AnyAsync(d => d.UserId == appointment.DoctorId);

                if (!patientExists || !doctorExists)
                {
                    return BadRequest(new { msg = "Invalid PatientId or DoctorId" });
                }

                _context.Entry(appointment).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return Ok(new { msg = "Appointment updated successfully", data = appointment });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { msg = "Error updating appointment", error = ex.Message });
            }
        }

        // DELETE: api/AdminAppointments/delete-appointment/{appointmentIdPublic}
        [HttpDelete("delete-appointment/{appointmentIdPublic}")]
        public async Task<IActionResult> DeleteAppointment(string appointmentIdPublic)
        {
            try
            {
                var appointment = await _context.Appointments
                    .FirstOrDefaultAsync(a => a.AppointmentIdPublic == appointmentIdPublic);

                if (appointment == null)
                {
                    return NotFound(new { msg = "Appointment not found" });
                }

                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();

                return Ok(new { msg = "Appointment deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { msg = "Error deleting appointment", error = ex.Message });
            }
        }
    }
}