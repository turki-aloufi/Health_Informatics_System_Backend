using Health_Informatics_System_Backend.Models;

namespace Health_Informatics_System_Backend.DTOs
{
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
