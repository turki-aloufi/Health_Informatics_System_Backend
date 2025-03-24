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
    [Authorize(Roles = "Patient")]
    public class PatientProfileController : ControllerBase
    {
        private readonly AppDbContext _context;
        public PatientProfileController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/PatientProfile/me
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            int userId = int.Parse(userIdClaim);
            var profile = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null)
            {
                return NotFound("Profile not found");
            }
            return Ok(profile);
        }


        [Authorize(Roles = "Patient")] // Only patients can access this
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdatePatientProfile([FromBody] UpdatePatientProfileDto updateDto){
        if (updateDto == null){
        return BadRequest("Invalid request data.");
        }

        // Get user ID from JWT token
        var userIdClaim = User.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(userIdClaim)){
        return Unauthorized("User not found.");
    }

        int userId = int.Parse(userIdClaim);

        var patientProfile = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
        if (patientProfile == null){
        return NotFound("Patient profile not found.");
    }

        // Update profile fields if new values are provided
        patientProfile.MedicalHistory = updateDto.MedicalHistory ?? patientProfile.MedicalHistory;
        patientProfile.InsuranceDetails = updateDto.InsuranceDetails ?? patientProfile.InsuranceDetails;
        patientProfile.EmergencyContact = updateDto.EmergencyContact ?? patientProfile.EmergencyContact;

        _context.PatientProfiles.Update(patientProfile);
        await _context.SaveChangesAsync();

        return Ok("Profile updated successfully.");
}
    }
}
