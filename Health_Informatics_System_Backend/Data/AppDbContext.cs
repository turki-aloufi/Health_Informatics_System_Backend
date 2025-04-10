using Microsoft.EntityFrameworkCore;
using Health_Informatics_System_Backend.Models;

namespace Health_Informatics_System_Backend.Data{
    public class AppDbContext : DbContext{
        public AppDbContext(DbContextOptions<AppDbContext> options): base(options){}

        //DbSets for the entties:
        public DbSet<User> Users {get; set;}
        public DbSet<PatientProfile> PatientProfiles {get; set;}
        public DbSet<DoctorProfile> DoctorProfiles {get; set;}
        public DbSet<DoctorAvailability> DoctorAvailabilities {get; set;}
        public DbSet<Appointment> Appointments {get; set;}
        public DbSet<Notification> Notifications {get; set;}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Configure one to one relationships for profiles using UserID as both PK & FK
            modelBuilder.Entity<PatientProfile>()
            .HasKey(pp => pp.UserId);
            modelBuilder.Entity<PatientProfile>()
            .HasOne(pp=> pp.User)
            .WithOne(u => u.PatientProfile)
            .HasForeignKey<PatientProfile>(pp => pp.UserId);

            modelBuilder.Entity<DoctorProfile>()
            .HasKey(dp => dp.UserId);
            modelBuilder.Entity<DoctorProfile>()
            .HasOne(dp => dp.User)
            .WithOne(u => u.DoctorProfile)
            .HasForeignKey<DoctorProfile>(dp => dp.UserId);

            //Configure Appointment relationship to disable cascade delete to avoid multiple cascade paths
            modelBuilder.Entity<Appointment>()
            .HasOne(a => a.PatientProfile)
            .WithMany()
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Appointment>()
            .HasOne(a => a.DoctorProfile)
            .WithMany(d => d.Appointments)
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DoctorAvailability>()
                .HasOne(da => da.DoctorProfile)
                .WithMany(dp => dp.Availabilities)
                .HasForeignKey(da => da.DoctorId);

            //Further customizations (if needed)

            base.OnModelCreating(modelBuilder);
        }
    }
    }