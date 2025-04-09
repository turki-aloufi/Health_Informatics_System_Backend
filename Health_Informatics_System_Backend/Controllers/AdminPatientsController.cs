using Health_Informatics_System_Backend.Data;
using Health_Informatics_System_Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using System.Text.Json;
using Health_Informatics_System_Backend.DTOs;

namespace Health_Informatics_System_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]

    public class AdminPatientsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public AdminPatientsController(AppDbContext context)
        {
            _context = context;
        }


        [HttpGet("get-all-patients")]
        public async Task<ActionResult> GetAllPatients()
        {
            var patients = await _context.PatientProfiles
                .Include(p => p.User)
                .Select(p => new
                {
                    idPublic = p.User.IdPublic,
                    name = p.User.Name,
                    email = p.User.Email,
                    role = p.User.Role,
                    doB = p.User.DoB,
                    ssn = p.User.SSN,
                    gender = p.User.Gender,
                    phoneNumber = p.User.PhoneNumber,
                    address = p.User.Address,
                    medicalHistory = p.MedicalHistory,
                    insuranceDetails = p.InsuranceDetails,
                    emergencyContact = p.EmergencyContact
                })
                .ToListAsync();

            var response = new
            {
                msg = "Retrieved patients",
                data = patients
            };
            return Ok(response);
        }

        // GET: api/PatientProfiles/get-patient-by-id/{id}
        [HttpGet("get-patient-by-id/{id}")]
        public async Task<ActionResult> GetPatientById(string id)
        {
            var patient = await _context.PatientProfiles
                .Include(p => p.User)
                                .Select(p => new
                                {
                                    idPublic = p.User.IdPublic,
                                    name = p.User.Name,
                                    email = p.User.Email,
                                    role = p.User.Role,
                                    doB = p.User.DoB,
                                    ssn = p.User.SSN,
                                    gender = p.User.Gender,
                                    phoneNumber = p.User.PhoneNumber,
                                    address = p.User.Address,
                                    medicalHistory = p.MedicalHistory,
                                    insuranceDetails = p.InsuranceDetails,
                                    emergencyContact = p.EmergencyContact
                                })
                .FirstOrDefaultAsync(p => p.idPublic == id);

            if (patient == null)
            {
                return NotFound(new
                {
                    msg = "Patient not found",
                    data = (object)null
                });
            }

            return Ok(new
            {
                msg = "Retrieved patient",
                data = patient
            });
        }



        // PUT: api/PatientProfiles/update-patient/{id}
        [HttpPut("update-patient/{id}")]
        public async Task<ActionResult> UpdatePatient(string id, [FromBody] AdminUpdatePatientDto patientDto)
        {
            if (patientDto == null)
            {
                return BadRequest("Invalid request data.");
            }

            var patientProfile = await _context.PatientProfiles
                          .Include(p => p.User)
                          .FirstOrDefaultAsync(p => p.User.IdPublic == id);

            if (patientProfile == null)
            {
                return NotFound("Patient profile not found.");
            }

            patientProfile.User.Name = patientDto.Name;
            patientProfile.User.PhoneNumber = patientDto.PhoneNumber;
            patientProfile.User.Address = patientDto.Address;
            patientProfile.User.DoB = patientDto.DoB;
            patientProfile.User.Email = patientDto.Email;
            patientProfile.User.Gender = patientDto.Gender;
            patientProfile.User.SSN = patientDto.SSN;
            patientProfile.MedicalHistory = patientDto.MedicalHistory ?? patientProfile.MedicalHistory;
            patientProfile.InsuranceDetails = patientDto.InsuranceDetails ?? patientProfile.InsuranceDetails;
            patientProfile.EmergencyContact = patientDto.EmergencyContact ?? patientProfile.EmergencyContact;

            _context.PatientProfiles.Update(patientProfile);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                msg = "Profile updated successfully.",
                data = patientDto
            });
        }


        // DELETE: api/PatientProfiles/delete-patient/{id}
        [HttpDelete("delete-patient/{id}")]
        public async Task<ActionResult> DeletePatient(string id)
        {
            var patient = await _context.Users.Where(user => user.IdPublic == id).FirstOrDefaultAsync();
            if (patient == null)
            {
                return NotFound(new
                {
                    msg = "Patient not found",
                    data = (object)null
                });
            }

            _context.Users.Remove(patient);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                msg = "Patient deleted successfully",
                data = (object)null
            });
        }
    }
}