using System;
using System.Collections.Generic;
using Health_Informatics_System_Backend.Models;

namespace Health_Informatics_System_Backend.DTOs
{
    public class DashboardSummaryDto
    {
        public int TotalAppointments { get; set; }
        public int WeeklyAppointments { get; set; }
        public AppointmentStatusCountDto AppointmentsByStatus { get; set; }
        public int TotalPatients { get; set; }
        public int WeeklyPatients { get; set; }
        public int TotalDoctors { get; set; }
        public GenderDistributionDto GenderDistribution { get; set; }
        public Dictionary<string, int> AgeDistribution { get; set; }
        public Dictionary<DayOfWeek, int> WeeklyAppointmentsByDay { get; set; }
    }

    public class AppointmentStatusCountDto
    {
        public int Scheduled { get; set; }
        public int Completed { get; set; }
        public int Cancelled { get; set; }
    }

    public class GenderDistributionDto
    {
        public int Male { get; set; }
        public int Female { get; set; }
    }

    public class AppointmentTrendDto
    {
        public string Date { get; set; }
        public int Count { get; set; }
    }

    public class DoctorWorkloadDto
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public int AppointmentCount { get; set; }
        public int CompletedCount { get; set; }
        public int ScheduledCount { get; set; }
        public int CancelledCount { get; set; }
    }

    public class AgeGroupDto
    {
        public string AgeGroup { get; set; }
        public int Count { get; set; }
    }

    public class GenderGroupDto
    {
        public Gender Gender { get; set; }
        public int Count { get; set; }
    }

    public class PatientsDemographicsDto
    {
        public List<GenderGroupDto> GenderDistribution { get; set; }
        public List<AgeGroupDto> AgeDistribution { get; set; }
    }
}