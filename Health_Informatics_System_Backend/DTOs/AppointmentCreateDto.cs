using global::Health_Informatics_System_Backend.Models;
using System.ComponentModel.DataAnnotations;
namespace Health_Informatics_System_Backend.DTOs
{
   

    namespace Health_Informatics_System_Backend.Dtos
    {
        public class AppointmentCreateDto
        {
            [Required]
            public int PatientId { get; set; }

            [Required]
            public int DoctorId { get; set; }

            [Required]
            public DateTime AppointmentDateTime { get; set; }

            [Required]
            public AppointmentStatus Status { get; set; }

            public string? Notes { get; set; }
        }
    }

}
