using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Health_Informatics_System_Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace Health_Informatics_System_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Doctor")]
    public class DoctorProfileController : ControllerBase
    {
        private readonly AppDbContext _context;
        public DoctorProfileController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/DoctorProfile/me
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            int userId = int.Parse(userIdClaim);
            var profile = await _context.DoctorProfiles.FirstOrDefaultAsync(d => d.UserId == userId);
            if (profile == null)
            {
                return NotFound("Profile not found");
            }
            return Ok(profile);
        }
    }
}
