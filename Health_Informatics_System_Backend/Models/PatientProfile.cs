using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Health_Informatics_System_Backend.Models{
public class PatientProfile
{
    [Key, ForeignKey("User")]
    public int UserId { get; set; }

    public string MedicalHistory { get; set; }
    public string InsuranceDetails { get; set; }
    public string EmergencyContact { get; set; }

    // Navigation Property
    public virtual User User { get; set; }
}
}