using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public enum UserRole { Patient, Doctor, Admin }
public enum Gender { Male, Female }

public class User
{
    [Key]
    public int Id { get; set; }
    public string IdPublic { get; set; } = Guid.NewGuid().ToString(); 

    [Required, MaxLength(100)]
    public string Name { get; set; }

    [Required, EmailAddress]
    public string Email { get; set; }

    [Required]
    public UserRole Role { get; set; }

    [Required]
    public string Password { get; set; } // Store hashed

    [Required]
    public DateTime DoB { get; set; }

    [Required, MaxLength(20)]
    public string SSN { get; set; } // Consider encryption

    [Required]
    public Gender Gender { get; set; }

    public string PhoneNumber { get; set; }
    public string Address { get; set; }

    // Navigation Properties
    public virtual PatientProfile PatientProfile { get; set; }
    public virtual DoctorProfile DoctorProfile { get; set; }
    public virtual ICollection<Notification> Notifications { get; set; }
}
