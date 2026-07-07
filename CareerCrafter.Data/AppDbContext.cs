using CareerCrafter.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace CareerCrafter.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<EmployerProfile> EmployerProfiles { get; set; }
        public DbSet<JobSeekerProfile> JobSeekerProfiles { get; set; }
        public DbSet<Education> Educations { get; set; }
        public DbSet<Experience> Experiences { get; set; }
        public DbSet<Resume> Resumes { get; set; }
        public DbSet<JobListing> JobListings { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        public DbSet<PasswordResetOtp> PasswordResetOtps { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            });

            // EmployerProfile
            modelBuilder.Entity<EmployerProfile>(entity =>
            {
                entity.HasKey(e => e.EmployerProfileId);
                entity.HasIndex(e => e.UserId).IsUnique();
                entity.Property(e => e.CompanyName).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Industry).HasMaxLength(100);
                entity.Property(e => e.Website).HasMaxLength(200);
                entity.Property(e => e.Location).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.HasOne(e => e.User)
                      .WithOne(u => u.EmployerProfile)
                      .HasForeignKey<EmployerProfile>(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // JobSeekerProfile
            modelBuilder.Entity<JobSeekerProfile>(entity =>
            {
                entity.HasKey(e => e.JobSeekerProfileId);
                entity.HasIndex(e => e.UserId).IsUnique();
                entity.Property(e => e.PhoneNumber).HasMaxLength(15);
                entity.Property(e => e.Location).HasMaxLength(100);
                entity.Property(e => e.Summary).HasMaxLength(1000);
                entity.Property(e => e.Skills).HasMaxLength(500);
                entity.HasOne(e => e.User)
                      .WithOne(u => u.JobSeekerProfile)
                      .HasForeignKey<JobSeekerProfile>(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Education
            modelBuilder.Entity<Education>(entity =>
            {
                entity.HasKey(e => e.EducationId);
                entity.Property(e => e.Degree).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Institution).IsRequired().HasMaxLength(150);
                entity.HasOne(e => e.JobSeekerProfile)
                      .WithMany(p => p.Educations)
                      .HasForeignKey(e => e.JobSeekerProfileId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Experience
            modelBuilder.Entity<Experience>(entity =>
            {
                entity.HasKey(e => e.ExperienceId);
                entity.Property(e => e.JobTitle).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Company).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Duration).HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.HasOne(e => e.JobSeekerProfile)
                      .WithMany(p => p.Experiences)
                      .HasForeignKey(e => e.JobSeekerProfileId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Resume
            modelBuilder.Entity<Resume>(entity =>
            {
                entity.HasKey(e => e.ResumeId);
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
                entity.Property(e => e.UploadedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.HasOne(e => e.JobSeekerProfile)
                      .WithMany(p => p.Resumes)
                      .HasForeignKey(e => e.JobSeekerProfileId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // JobListing
            modelBuilder.Entity<JobListing>(entity =>
            {
                entity.HasKey(e => e.JobId);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(2000);
                entity.Property(e => e.Location).HasMaxLength(100);
                entity.Property(e => e.JobType).HasMaxLength(50);
                entity.Property(e => e.SalaryRange).HasMaxLength(50);
                entity.Property(e => e.RequiredSkills).HasMaxLength(500);
                entity.Property(e => e.PostedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.HasOne(e => e.EmployerProfile)
                      .WithMany(p => p.JobListings)
                      .HasForeignKey(e => e.EmployerProfileId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Application
            modelBuilder.Entity<Application>(entity =>
            {
                entity.HasKey(e => e.ApplicationId);
                entity.Property(e => e.Status).HasMaxLength(20).HasDefaultValue("Pending");
                entity.Property(e => e.AppliedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.CoverNote).HasMaxLength(1000);
                entity.HasOne(e => e.Job)
                      .WithMany(p => p.Applications)
                      .HasForeignKey(e => e.JobId)
                      .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.JobSeekerProfile)
                      .WithMany(p => p.Applications)
                      .HasForeignKey(e => e.JobSeekerProfileId)
                      .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(e => e.Resume)
                      .WithMany(p => p.Applications)
                      .HasForeignKey(e => e.ResumeId)
                      .OnDelete(DeleteBehavior.NoAction);
            });

            // Notification
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.NotificationId);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(500);
                entity.Property(e => e.IsRead).HasDefaultValue(false);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Notifications)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // PasswordResetOtp
            modelBuilder.Entity<PasswordResetOtp>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.OtpCode)
                      .IsRequired()
                      .HasMaxLength(6);

                entity.Property(e => e.CreatedAt)
                      .HasDefaultValueSql("GETDATE()");

                entity.Property(e => e.IsUsed)
                      .HasDefaultValue(false);

                entity.HasOne(e => e.User)
                      .WithMany(u => u.PasswordResetOtps)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}