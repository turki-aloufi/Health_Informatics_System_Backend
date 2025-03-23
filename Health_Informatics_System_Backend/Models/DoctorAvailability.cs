using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class DoctorAvailability
{
    [Key]
    public int AvailabilityId { get; set; }
    public string AvailabilityIdPublic { get; set; } = Guid.NewGuid().ToString(); 


    [ForeignKey("DoctorProfile")]
    public int DoctorId { get; set; }

    [Required]
    public int DayOfWeek { get; set; } // 1 = Monday, 7 = Sunday

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    // Navigation Property
    public virtual DoctorProfile DoctorProfile { get; set; }
}
