using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class DoctorProfile
{
    [Key, ForeignKey("User")]
    public int UserId { get; set; }

    [Required, MaxLength(100)]
    public string Specialty { get; set; }

    [Required, MaxLength(50)]
    public string LicenseNumber { get; set; }

    public string Clinic { get; set; }

    // Navigation Properties
    public virtual User User { get; set; }
    public virtual ICollection<DoctorAvailability> Availabilities { get; set; }
    public virtual ICollection<Appointment> Appointments { get; set; }
}
