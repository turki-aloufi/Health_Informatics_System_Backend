using Health_Informatics_System_Backend.Models;

namespace Health_Informatics_System_Backend.DTOs
{
    public class AdminUpdatePatientDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime DoB { get; set; }
        public string SSN { get; set; }
        public Gender Gender { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string MedicalHistory { get; set; }
        public string InsuranceDetails { get; set; }
        public string EmergencyContact { get; set; }
    }
}
