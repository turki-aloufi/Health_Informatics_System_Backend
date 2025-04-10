using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Health_Informatics_System_Backend.Data;
using Health_Informatics_System_Backend.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Health_Informatics_System_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Dashboard/summary
        [HttpGet("summary")]
        public async Task<IActionResult> GetDashboardSummary()
        {
            var now = DateTime.Now;
            var startOfWeek = now.AddDays(-(int)now.DayOfWeek).Date;
            var endOfWeek = startOfWeek.AddDays(7);

            // Appointments statistics
            var totalAppointments = await _context.Appointments.CountAsync();
            var weeklyAppointments = await _context.Appointments
                .Where(a => a.AppointmentDateTime >= startOfWeek && a.AppointmentDateTime < endOfWeek)
                .CountAsync();

            // Appointments by status
            var scheduledAppointments = await _context.Appointments
                .Where(a => a.Status == AppointmentStatus.Scheduled)
                .CountAsync();
            var completedAppointments = await _context.Appointments
                .Where(a => a.Status == AppointmentStatus.Completed)
                .CountAsync();
            var cancelledAppointments = await _context.Appointments
                .Where(a => a.Status == AppointmentStatus.Cancelled)
                .CountAsync();

            // User statistics
            var totalPatients = await _context.Users
                .Where(u => u.Role == UserRole.Patient)
                .CountAsync();
            var totalDoctors = await _context.Users
                .Where(u => u.Role == UserRole.Doctor)
                .CountAsync();

            // Weekly new patients (assuming Id is auto-incrementing and higher Id means newer registration)
            var weeklyPatients = await _context.Users
                .Where(u => u.Role == UserRole.Patient)
                .OrderByDescending(u => u.Id)
                .Take(totalPatients / 10) // Approximating weekly patients as 10% of total for demo
                .CountAsync();

            // Gender distribution
            var malePatients = await _context.Users
                .Where(u => u.Role == UserRole.Patient && u.Gender == Gender.Male)
                .CountAsync();
            var femalePatients = await _context.Users
                .Where(u => u.Role == UserRole.Patient && u.Gender == Gender.Female)
                .CountAsync();

            // Age distribution
            var ageGroups = await _context.Users
                .Where(u => u.Role == UserRole.Patient)
                .Select(u => new { Age = (now.Year - u.DoB.Year) / 10 * 10 }) // Group by decade, calculate client-side
                .ToListAsync();

            // Now group the ages client-side
            var ageDistribution = ageGroups
                .GroupBy(a => a.Age)
                .Select(g => new { AgeGroup = g.Key, Count = g.Count() })
                .ToDictionary(
                    x => $"{x.AgeGroup}-{x.AgeGroup + 9}",
                    x => x.Count
                );

            // Get weekly appointments for grouping by day of week
            // First, get the raw appointments data from the database
            var weeklyAppointmentsData = await _context.Appointments
                .Where(a => a.AppointmentDateTime >= startOfWeek && a.AppointmentDateTime < endOfWeek)
                .Select(a => new { a.AppointmentDateTime }) // Select only what we need
                .ToListAsync(); // Execute query and bring data to memory

            // Now perform the grouping operation in memory (client-side)
            var weeklyAppointmentsByDay = weeklyAppointmentsData
                .GroupBy(a => a.AppointmentDateTime.DayOfWeek)
                .Select(g => new { Day = g.Key, Count = g.Count() })
                .ToList();

            var daysOfWeek = new Dictionary<DayOfWeek, int>
            {
                { DayOfWeek.Sunday, 0 },
                { DayOfWeek.Monday, 0 },
                { DayOfWeek.Tuesday, 0 },
                { DayOfWeek.Wednesday, 0 },
                { DayOfWeek.Thursday, 0 },
                { DayOfWeek.Friday, 0 },
                { DayOfWeek.Saturday, 0 }
            };

            foreach (var day in weeklyAppointmentsByDay)
            {
                daysOfWeek[day.Day] = day.Count;
            }

            var response = new
            {
                TotalAppointments = totalAppointments,
                WeeklyAppointments = weeklyAppointments,
                AppointmentsByStatus = new
                {
                    Scheduled = scheduledAppointments,
                    Completed = completedAppointments,
                    Cancelled = cancelledAppointments
                },
                TotalPatients = totalPatients,
                WeeklyPatients = weeklyPatients,
                TotalDoctors = totalDoctors,
                GenderDistribution = new
                {
                    Male = malePatients,
                    Female = femalePatients
                },
                AgeDistribution = ageDistribution,
                WeeklyAppointmentsByDay = daysOfWeek
            };

            return Ok(response);
        }

        // GET: api/Dashboard/appointments-trend
        [HttpGet("appointments-trend")]
        public async Task<IActionResult> GetAppointmentsTrend([FromQuery] int days = 30)
        {
            var startDate = DateTime.Now.AddDays(-days).Date;
            var endDate = DateTime.Now.Date.AddDays(1);

            // Get raw appointment data
            var appointmentsData = await _context.Appointments
                .Where(a => a.AppointmentDateTime >= startDate && a.AppointmentDateTime < endDate)
                .Select(a => new { Date = a.AppointmentDateTime.Date })
                .ToListAsync();

            // Group by date in memory
            var appointmentsByDate = appointmentsData
                .GroupBy(a => a.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToList();

            // Fill in missing dates with zero counts
            var allDates = new List<object>();
            for (var date = startDate; date < endDate; date = date.AddDays(1))
            {
                var found = appointmentsByDate.FirstOrDefault(a => a.Date == date);
                allDates.Add(new
                {
                   Date = date.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),                    
                   Count = found?.Count ?? 0
                });
            }

            return Ok(new { Trend = allDates });
        }

        // GET: api/Dashboard/doctors-workload
        [HttpGet("doctors-workload")]
        public async Task<IActionResult> GetDoctorsWorkload()
        {
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1);

            // Get all appointments for the current month with status info
            var appointmentsWithStatus = await _context.Appointments
                .Where(a => a.AppointmentDateTime >= startOfMonth && a.AppointmentDateTime < endOfMonth)
                .Select(a => new 
                { 
                    a.DoctorId, 
                    a.Status 
                })
                .ToListAsync();

            // Group and calculate stats in memory
            var doctorWorkloads = appointmentsWithStatus
                .GroupBy(a => a.DoctorId)
                .Select(g => new
                {
                    DoctorId = g.Key,
                    AppointmentCount = g.Count(),
                    CompletedCount = g.Count(a => a.Status == AppointmentStatus.Completed),
                    ScheduledCount = g.Count(a => a.Status == AppointmentStatus.Scheduled),
                    CancelledCount = g.Count(a => a.Status == AppointmentStatus.Cancelled)
                })
                .ToList();

            // Fetch doctor names
            var doctorNames = await _context.DoctorProfiles
                .Include(d => d.User)
                .ToDictionaryAsync(d => d.UserId, d => d.User.Name);

            var result = doctorWorkloads.Select(w => new
            {
                DoctorId = w.DoctorId,
                DoctorName = doctorNames.ContainsKey(w.DoctorId) ? doctorNames[w.DoctorId] : "Unknown",
                AppointmentCount = w.AppointmentCount,
                CompletedCount = w.CompletedCount,
                ScheduledCount = w.ScheduledCount,
                CancelledCount = w.CancelledCount
            });

            return Ok(new { DoctorWorkloads = result });
        }

        // GET: api/Dashboard/patients-demographics
        [HttpGet("patients-demographics")]
        public async Task<IActionResult> GetPatientsDemographics()
        {
            var now = DateTime.Now;

            // Gender distribution - simple count query is OK
            var genderDistribution = await _context.Users
                .Where(u => u.Role == UserRole.Patient)
                .GroupBy(u => u.Gender)
                .Select(g => new { Gender = g.Key, Count = g.Count() })
                .ToListAsync();

            // For the age groups, we need to load the data first and then do the grouping in memory
            var patients = await _context.Users
                .Where(u => u.Role == UserRole.Patient)
                .Select(u => new { Age = now.Year - u.DoB.Year })
                .ToListAsync();

            var ageGroups = new[]
            {
                new { Label = "0-17", Min = 0, Max = 17 },
                new { Label = "18-24", Min = 18, Max = 24 },
                new { Label = "25-34", Min = 25, Max = 34 },
                new { Label = "35-44", Min = 35, Max = 44 },
                new { Label = "45-54", Min = 45, Max = 54 },
                new { Label = "55-64", Min = 55, Max = 64 },
                new { Label = "65+", Min = 65, Max = 200 }
            };

            var ageDistribution = ageGroups
                .Select(group => new
                {
                    AgeGroup = group.Label,
                    Count = patients.Count(p => p.Age >= group.Min && p.Age <= group.Max)
                })
                .ToList();

            return Ok(new
            {
                GenderDistribution = genderDistribution,
                AgeDistribution = ageDistribution
            });
        }
    }
}