using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Health_Informatics_System_Backend.Data;
using Health_Informatics_System_Backend.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Health_Informatics_System_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DoctorsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DoctorsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Doctors
        [HttpGet]
        public async Task<IActionResult> GetAllDoctors()
        {
            var doctors = await _context.DoctorProfiles
                .AsNoTracking()
                .Include(dp => dp.User) 
                .Select(dp => new DoctorDto
                {
                    DoctorId = dp.UserId,
                    Name = dp.User.Name,        
                    Specialty = dp.Specialty,
                    Clinic = dp.Clinic
                })
                .ToListAsync();

            return Ok(doctors);
        }
    }
}
