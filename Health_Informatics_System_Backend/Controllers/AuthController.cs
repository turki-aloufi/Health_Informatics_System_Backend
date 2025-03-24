using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Health_Informatics_System_Backend.Data;
using Health_Informatics_System_Backend.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace Health_Informatics_System_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto){
        // Retrieve the user by email
         var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
         if (user == null) {
        return Unauthorized("Invalid credentials");
        }
    
        // Verify the password using BCrypt
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
        {
        return Unauthorized("Invalid credentials");
        }

        var token = GenerateJwtToken(user);
        return Ok(new { Token = token });
}

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            // Check if the email is already registered
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
            {
                return BadRequest("A user with this email already exists.");
            }

            // Create new user; auto assign role as Patient
            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                // In production, use a proper hashing mechanism.
                Password = HashPassword(dto.Password),
                DoB = dto.DoB,
                SSN = dto.SSN,
                Gender = dto.Gender,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
                Role = UserRole.Patient
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Create an associated PatientProfile (with empty/default values)
            var patientProfile = new PatientProfile
            {
                UserId = user.Id,
                MedicalHistory = string.Empty,
                InsuranceDetails = null, // optional
                EmergencyContact = string.Empty
            };

            _context.PatientProfiles.Add(patientProfile);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Register), new { id = user.Id }, new { message = "Registration successful", userId = user.Id });
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim("UserId", user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string HashPassword(string password)
        {
                if (string.IsNullOrEmpty(password)){
                     throw new ArgumentException("Password cannot be null or empty.");
                }
             return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }

    // DTO for login
    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    // DTO for registration
    public class RegisterDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime DoB { get; set; }
        public string SSN { get; set; }
        public Gender Gender { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
    }
}
