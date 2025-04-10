using Health_Informatics_System_Backend.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Health_Informatics_System_Backend.DTOs
{
    public class DoctorAvailabilityDto
    {
        public int DayOfWeek { get; set; } // 1 = Monday, 7 = Sunday
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }

    public class AdminUpdateDoctorDto
    {
        [Required]
        public string Name { get; set; }
        
        [Required, EmailAddress]
        public string Email { get; set; }
        
        [Required]
        public DateTime DoB { get; set; }
        
        [Required]
        public string SSN { get; set; }
        
        [Required]
        public Gender Gender { get; set; }
        
        public string PhoneNumber { get; set; }
        
        public string Address { get; set; }
        
        [Required]
        public string Specialty { get; set; }
        
        [Required]
        public string LicenseNumber { get; set; }
        
        public string Clinic { get; set; }
        
        public List<DoctorAvailabilityDto> Availabilities { get; set; }
    }

    public class AdminCreateDoctorDto : AdminUpdateDoctorDto
    {
        [Required]
        public string Password { get; set; }
    }
}