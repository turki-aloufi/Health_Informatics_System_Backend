using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Health_Informatics_System_Backend.Data;
using Health_Informatics_System_Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace Health_Informatics_System_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Patient")]
    public class PatientProfileController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<PatientProfileController> _logger;

        public PatientProfileController(AppDbContext context, IMemoryCache cache, ILogger<PatientProfileController> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            int userId = int.Parse(userIdClaim);
            string cacheKey = $"PatientProfile_{userId}";

            if (_cache.TryGetValue(cacheKey, out PatientProfile profile))
            {
                _logger.LogInformation("Cache hit for user ID: {UserId}", userId);
            }
            else
            {
                _logger.LogInformation("Cache miss for user ID: {UserId}", userId);
                profile = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
                if (profile == null)
                    return NotFound("Profile not found");

                _cache.Set(cacheKey, profile, TimeSpan.FromMinutes(5));
            }


            return Ok(profile);
        }


        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdatePatientProfile([FromBody] UpdatePatientProfileDto updateDto)
        {
            if (updateDto == null)
                return BadRequest("Invalid request data.");

            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized("User not found.");

            int userId = int.Parse(userIdClaim);

            var patientProfile = await _context.PatientProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
            if (patientProfile == null)
                return NotFound("Patient profile not found.");

            patientProfile.MedicalHistory = updateDto.MedicalHistory ?? patientProfile.MedicalHistory;
            patientProfile.InsuranceDetails = updateDto.InsuranceDetails ?? patientProfile.InsuranceDetails;
            patientProfile.EmergencyContact = updateDto.EmergencyContact ?? patientProfile.EmergencyContact;

            _context.PatientProfiles.Update(patientProfile);
            await _context.SaveChangesAsync();

            // Clear the cache after update
            string cacheKey = $"PatientProfile_{userId}";
            _cache.Remove(cacheKey);

            return Ok("Profile updated successfully.");
        }
    }
}
