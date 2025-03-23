﻿// <auto-generated />
using System;
using Health_Informatics_System_Backend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Health_Informatics_System_Backend.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Health_Informatics_System_Backend.Models.Appointment", b =>
                {
                    b.Property<int>("AppointmentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("AppointmentId"));

                    b.Property<DateTime>("AppointmentDateTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("AppointmentIdPublic")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("DoctorId")
                        .HasColumnType("int");

                    b.Property<string>("Notes")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("PatientId")
                        .HasColumnType("int");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.HasKey("AppointmentId");

                    b.HasIndex("DoctorId");

                    b.HasIndex("PatientId");

                    b.ToTable("Appointments");
                });

            modelBuilder.Entity("Health_Informatics_System_Backend.Models.DoctorAvailability", b =>
                {
                    b.Property<int>("AvailabilityId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("AvailabilityId"));

                    b.Property<string>("AvailabilityIdPublic")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("DayOfWeek")
                        .HasColumnType("int");

                    b.Property<int>("DoctorId")
                        .HasColumnType("int");

                    b.Property<TimeSpan>("EndTime")
                        .HasColumnType("time");

                    b.Property<TimeSpan>("StartTime")
                        .HasColumnType("time");

                    b.HasKey("AvailabilityId");

                    b.HasIndex("DoctorId");

                    b.ToTable("DoctorAvailabilities");
                });

            modelBuilder.Entity("Health_Informatics_System_Backend.Models.DoctorProfile", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<string>("Clinic")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LicenseNumber")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Specialty")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("UserId");

                    b.ToTable("DoctorProfiles");
                });

            modelBuilder.Entity("Health_Informatics_System_Backend.Models.Notification", b =>
                {
                    b.Property<int>("NotificationId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("NotificationId"));

                    b.Property<string>("Message")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NotificationIdPublic")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("SentAt")
                        .HasColumnType("datetime2");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("NotificationId");

                    b.HasIndex("UserId");

                    b.ToTable("Notifications");
                });

            modelBuilder.Entity("Health_Informatics_System_Backend.Models.PatientProfile", b =>
                {
                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<string>("EmergencyContact")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("InsuranceDetails")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("MedicalHistory")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId");

                    b.ToTable("PatientProfiles");
                });

            modelBuilder.Entity("Health_Informatics_System_Backend.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("DoB")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Gender")
                        .HasColumnType("int");

                    b.Property<string>("IdPublic")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Role")
                        .HasColumnType("int");

                    b.Property<string>("SSN")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("nvarchar(20)");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Health_Informatics_System_Backend.Models.Appointment", b =>
                {
                    b.HasOne("Health_Informatics_System_Backend.Models.DoctorProfile", "DoctorProfile")
                        .WithMany("Appointments")
                        .HasForeignKey("DoctorId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Health_Informatics_System_Backend.Models.PatientProfile", "PatientProfile")
                        .WithMany()
                        .HasForeignKey("PatientId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("DoctorProfile");

                    b.Navigation("PatientProfile");
                });

            modelBuilder.Entity("Health_Informatics_System_Backend.Models.DoctorAvailability", b =>
                {
                    b.HasOne("Health_Informatics_System_Backend.Models.DoctorProfile", "DoctorProfile")
                        .WithMany("Availabilities")
                        .HasForeignKey("DoctorId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DoctorProfile");
                });

            modelBuilder.Entity("Health_Informatics_System_Backend.Models.DoctorProfile", b =>
                {
                    b.HasOne("Health_Informatics_System_Backend.Models.User", "User")
                        .WithOne("DoctorProfile")
                        .HasForeignKey("Health_Informatics_System_Backend.Models.DoctorProfile", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Health_Informatics_System_Backend.Models.Notification", b =>
                {
                    b.HasOne("Health_Informatics_System_Backend.Models.User", "User")
                        .WithMany("Notifications")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Health_Informatics_System_Backend.Models.PatientProfile", b =>
                {
                    b.HasOne("Health_Informatics_System_Backend.Models.User", "User")
                        .WithOne("PatientProfile")
                        .HasForeignKey("Health_Informatics_System_Backend.Models.PatientProfile", "UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Health_Informatics_System_Backend.Models.DoctorProfile", b =>
                {
                    b.Navigation("Appointments");

                    b.Navigation("Availabilities");
                });

            modelBuilder.Entity("Health_Informatics_System_Backend.Models.User", b =>
                {
                    b.Navigation("DoctorProfile")
                        .IsRequired();

                    b.Navigation("Notifications");

                    b.Navigation("PatientProfile")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
