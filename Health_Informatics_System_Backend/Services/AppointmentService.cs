using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Health_Informatics_System_Backend.Data;
using Health_Informatics_System_Backend.Models;

namespace Health_Informatics_System_Backend.Services
{
    public class AppointmentService
    {
        private readonly AppDbContext _context;

        public AppointmentService(AppDbContext context)
        {
            _context = context;
        }

        // Check if the appointment time falls within the doctor’s available schedule
        public async Task<bool> CheckTimeSlotAvailability(int doctorId, DateTime appointmentDateTime)
        {
            // Find doctor availability for the day of the week (DayOfWeek: 0 = Sunday, 1 = Monday, etc.)
            var doctorAvailability = await _context.DoctorAvailabilities
                .FirstOrDefaultAsync(da => da.DoctorId == doctorId && da.DayOfWeek == ((int)appointmentDateTime.DayOfWeek == 0 ? 7 : (int)appointmentDateTime.DayOfWeek));
            
            if (doctorAvailability == null)
                return false;

            // Check if the appointment time is within the available window.
            var appointmentTime = appointmentDateTime.TimeOfDay;
            if (appointmentTime < doctorAvailability.StartTime || appointmentTime >= doctorAvailability.EndTime)
                return false;

            // Check if an appointment already exists at this time (ignoring cancelled ones).
            var existingAppointment = await _context.Appointments
                .FirstOrDefaultAsync(a => 
                    a.DoctorId == doctorId &&
                    a.AppointmentDateTime == appointmentDateTime &&
                    a.Status != AppointmentStatus.Cancelled);

            return existingAppointment == null;
        }

        // Create an appointment if the time slot is available.
        public async Task<Appointment> CreateAppointment(int patientId, int doctorId, DateTime appointmentDateTime, string notes = "")
        {
            if (!await CheckTimeSlotAvailability(doctorId, appointmentDateTime))
                throw new InvalidOperationException("The selected time slot is not available.");

            var appointment = new Appointment
            {
                PatientId = patientId,
                DoctorId = doctorId,
                AppointmentDateTime = appointmentDateTime,
                Status = AppointmentStatus.Scheduled,
                Notes = notes
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return appointment;
        }

        // (Optional) Generate available 30-minute slots for a given doctor on a specific date.
        public async Task<IQueryable<DateTime>> GetAvailableTimeSlots(int doctorId, DateTime date)
        {
            var availability = await _context.DoctorAvailabilities
                .FirstOrDefaultAsync(da => da.DoctorId == doctorId && da.DayOfWeek == ((int)date.DayOfWeek == 0 ? 7 : (int)date.DayOfWeek));

            if (availability == null)
                return Enumerable.Empty<DateTime>().AsQueryable();

            var startTime = date.Date + availability.StartTime;
            var endTime = date.Date + availability.EndTime;

            // Generate 30-minute slots and filter out booked slots.
            var timeSlots = Enumerable.Range(0, (int)((endTime - startTime).TotalMinutes / 30))
                .Select(i => startTime.AddMinutes(i * 30))
                .Where(slot => !_context.Appointments.Any(a =>
                    a.DoctorId == doctorId &&
                    a.AppointmentDateTime == slot &&
                    a.Status != AppointmentStatus.Cancelled));

            return timeSlots.AsQueryable();
        }
    }
}
