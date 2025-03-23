using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Health_Informatics_System_Backend.Models{
public enum AppointmentStatus { Scheduled, Completed, Cancelled }

public class Appointment
{
    [Key]
    public int AppointmentId { get; set; }
    public string AppointmentIdPublic { get; set; } = Guid.NewGuid().ToString(); 


    [ForeignKey("PatientProfile")]
    public int PatientId { get; set; }

    [ForeignKey("DoctorProfile")]
    public int DoctorId { get; set; }

    [Required]
    public DateTime AppointmentDateTime { get; set; }

    [Required]
    public AppointmentStatus Status { get; set; }

    public string Notes { get; set; }

    // Navigation Properties
    public virtual PatientProfile PatientProfile { get; set; }
    public virtual DoctorProfile DoctorProfile { get; set; }
}
}