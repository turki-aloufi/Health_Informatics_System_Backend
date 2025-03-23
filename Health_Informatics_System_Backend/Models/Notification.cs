using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Health_Informatics_System_Backend.Models{
public enum NotificationStatus { Sent, Failed, Pending }

public class Notification
{
    [Key]
    public int NotificationId { get; set; }
    public string NotificationIdPublic { get; set; } = Guid.NewGuid().ToString(); 


    [ForeignKey("User")]
    public int UserId { get; set; }

    [Required]
    public string Message { get; set; }

    public DateTime SentAt { get; set; }


    // Navigation Property
    public virtual User User { get; set; }
}
}