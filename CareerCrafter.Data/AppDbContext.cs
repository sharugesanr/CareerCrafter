using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CareerCrafter.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Application> Applications { get; set; }

    public virtual DbSet<Education> Educations { get; set; }

    public virtual DbSet<EmployerProfile> EmployerProfiles { get; set; }

    public virtual DbSet<Experience> Experiences { get; set; }

    public virtual DbSet<JobListing> JobListings { get; set; }

    public virtual DbSet<JobSeekerProfile> JobSeekerProfiles { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Resume> Resumes { get; set; }

    public virtual DbSet<User> Users { get; set; }

    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Application>(entity =>
        {
            entity.HasKey(e => e.ApplicationId).HasName("PK__Applicat__C93A4C995468129B");

            entity.Property(e => e.AppliedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CoverNote).HasMaxLength(1000);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.Job).WithMany(p => p.Applications)
                .HasForeignKey(d => d.JobId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Applicati__JobId__6A30C649");

            entity.HasOne(d => d.JobSeekerProfile).WithMany(p => p.Applications)
                .HasForeignKey(d => d.JobSeekerProfileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Applicati__JobSe__6B24EA82");

            entity.HasOne(d => d.Resume).WithMany(p => p.Applications)
                .HasForeignKey(d => d.ResumeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Applicati__Resum__6C190EBB");
        });

        modelBuilder.Entity<Education>(entity =>
        {
            entity.HasKey(e => e.EducationId).HasName("PK__Educatio__4BBE3805F59ECF09");

            entity.ToTable("Education");

            entity.Property(e => e.Degree).HasMaxLength(150);
            entity.Property(e => e.Institution).HasMaxLength(150);

            entity.HasOne(d => d.JobSeekerProfile).WithMany(p => p.Educations)
                .HasForeignKey(d => d.JobSeekerProfileId)
                .HasConstraintName("FK__Education__JobSe__571DF1D5");
        });

        modelBuilder.Entity<EmployerProfile>(entity =>
        {
            entity.HasKey(e => e.EmployerProfileId).HasName("PK__Employer__2C8ED6DB1ED9C739");

            entity.HasIndex(e => e.UserId, "UQ__Employer__1788CC4D8EABA1B1").IsUnique();

            entity.Property(e => e.CompanyName).HasMaxLength(150);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Industry).HasMaxLength(100);
            entity.Property(e => e.Location).HasMaxLength(100);
            entity.Property(e => e.Website).HasMaxLength(200);

            entity.HasOne(d => d.User).WithOne(p => p.EmployerProfile)
                .HasForeignKey<EmployerProfile>(d => d.UserId)
                .HasConstraintName("FK__EmployerP__UserI__5070F446");
        });

        modelBuilder.Entity<Experience>(entity =>
        {
            entity.HasKey(e => e.ExperienceId).HasName("PK__Experien__2F4E3449D97C84BE");

            entity.ToTable("Experience");

            entity.Property(e => e.Company).HasMaxLength(150);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Duration).HasMaxLength(50);
            entity.Property(e => e.JobTitle).HasMaxLength(100);

            entity.HasOne(d => d.JobSeekerProfile).WithMany(p => p.Experiences)
                .HasForeignKey(d => d.JobSeekerProfileId)
                .HasConstraintName("FK__Experienc__JobSe__59FA5E80");
        });

        modelBuilder.Entity<JobListing>(entity =>
        {
            entity.HasKey(e => e.JobId).HasName("PK__JobListi__056690C213DBB5FB");

            entity.Property(e => e.Deadline).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.JobType).HasMaxLength(50);
            entity.Property(e => e.Location).HasMaxLength(100);
            entity.Property(e => e.PostedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.RequiredSkills).HasMaxLength(500);
            entity.Property(e => e.SalaryRange).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(150);

            entity.HasOne(d => d.EmployerProfile).WithMany(p => p.JobListings)
                .HasForeignKey(d => d.EmployerProfileId)
                .HasConstraintName("FK__JobListin__Emplo__6477ECF3");
        });

        modelBuilder.Entity<JobSeekerProfile>(entity =>
        {
            entity.HasKey(e => e.JobSeekerProfileId).HasName("PK__JobSeeke__AB9350FC695E3532");

            entity.HasIndex(e => e.UserId, "UQ__JobSeeke__1788CC4DFDA6F11B").IsUnique();

            entity.Property(e => e.Location).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber).HasMaxLength(15);
            entity.Property(e => e.Skills).HasMaxLength(500);
            entity.Property(e => e.Summary).HasMaxLength(1000);

            entity.HasOne(d => d.User).WithOne(p => p.JobSeekerProfile)
                .HasForeignKey<JobSeekerProfile>(d => d.UserId)
                .HasConstraintName("FK__JobSeeker__UserI__5441852A");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E1278198784");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsRead).HasDefaultValue(false);
            entity.Property(e => e.Message).HasMaxLength(500);

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Notificat__UserI__70DDC3D8");
        });

        modelBuilder.Entity<Resume>(entity =>
        {
            entity.HasKey(e => e.ResumeId).HasName("PK__Resumes__D7D7A0F7F03AC582");

            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.FilePath).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.JobSeekerProfile).WithMany(p => p.Resumes)
                .HasForeignKey(d => d.JobSeekerProfileId)
                .HasConstraintName("FK__Resumes__JobSeek__5EBF139D");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C20A7DEB8");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534FA896436").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Role).HasMaxLength(20);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
