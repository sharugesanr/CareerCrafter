using CareerCrafter.Core.DTOs;
using CareerCrafter.Core.Models;
using CareerCrafter.Repositories.Interfaces;
using CareerCrafter.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CareerCrafter.Services.Implementations
{
    public class ApplicationService : IApplicationService
    {
        private readonly IApplicationRepository _applicationRepo;
        private readonly IJobSeekerRepository _jobSeekerRepo;
        private readonly IJobListingRepository _jobListingRepo;
        private readonly IEmployerRepository _employerRepo;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;

        public ApplicationService(
            IApplicationRepository applicationRepo,
            IJobSeekerRepository jobSeekerRepo,
            IJobListingRepository jobListingRepo,
            IEmployerRepository employerRepo,
            IEmailService emailService,
            INotificationService notificationService)
        {
            _applicationRepo = applicationRepo;
            _jobSeekerRepo = jobSeekerRepo;
            _jobListingRepo = jobListingRepo;
            _employerRepo = employerRepo;
            _emailService = emailService;
            _notificationService = notificationService;
        }
        private static ApplicationDto MapToApplicationDto(Application application)
        {
            return new ApplicationDto
            {
                ApplicationId = application.ApplicationId,
                JobId = application.JobId,
                JobTitle = application.Job.Title,
                CompanyName = application.Job.EmployerProfile.CompanyName,
                Status = application.Status,
                AppliedAt = application.AppliedAt,
                CoverNote = application.CoverNote,
                ResumeId = application.ResumeId
            };
        }

        private static ApplicantDto MapToApplicantDto(Application application)
        {
            return new ApplicantDto
            {
                ApplicationId = application.ApplicationId,
                JobSeekerName = application.JobSeekerProfile.User.FullName,
                JobSeekerEmail = application.JobSeekerProfile.User.Email,
                Status = application.Status,
                AppliedAt = application.AppliedAt,
                CoverNote = application.CoverNote,
                ResumeId = application.ResumeId
            };
        }

        public async Task<ApplicationDto> ApplyAsync(int userId, CreateApplicationDto dto)
        {
            var jobSeekerProfile = await _jobSeekerRepo.GetProfileByUserIdAsync(userId);
            if (jobSeekerProfile == null)
                throw new Exception("Job seeker profile not found.");

            var job = await _jobListingRepo.GetByIdAsync(dto.JobId);
            if (job == null || job.IsActive == false)
                throw new Exception("Job listing not found or is no longer active.");

            var resume = jobSeekerProfile.Resumes
                .FirstOrDefault(r => r.ResumeId == dto.ResumeId);
            if (resume == null)
                throw new Exception("Resume not found. Please upload a resume before applying.");

            var existing = await _applicationRepo.GetByJobAndJobSeekerAsync(dto.JobId, jobSeekerProfile.JobSeekerProfileId);
            if (existing != null)
                throw new Exception("You have already applied for this job.");

            var application = new Application
            {
                JobId = dto.JobId,
                JobSeekerProfileId = jobSeekerProfile.JobSeekerProfileId,
                ResumeId = dto.ResumeId,
                CoverNote = dto.CoverNote,
                Status = "Applied",
                AppliedAt = DateTime.Now
            };

            await _applicationRepo.AddAsync(application);
            await _applicationRepo.SaveChangesAsync();

            var saved = await _applicationRepo.GetByIdAsync(application.ApplicationId);
            return MapToApplicationDto(saved!);
        }

        public async Task<List<ApplicationDto>> GetMyApplicationsAsync(int userId)
        {
            var jobSeekerProfile = await _jobSeekerRepo.GetProfileByUserIdAsync(userId);
            if (jobSeekerProfile == null)
                throw new Exception("Job seeker profile not found.");

            var applications = await _applicationRepo.GetByJobSeekerProfileIdAsync(jobSeekerProfile.JobSeekerProfileId);
            return applications.Select(a => MapToApplicationDto(a)).ToList();
        }

        public async Task<ApplicationDto> GetApplicationByIdAsync(int userId, int applicationId)
        {
            var jobSeekerProfile = await _jobSeekerRepo.GetProfileByUserIdAsync(userId);
            if (jobSeekerProfile == null)
                throw new Exception("Job seeker profile not found.");

            var application = await _applicationRepo.GetByIdAsync(applicationId);
            if (application == null)
                throw new Exception("Application not found.");

            if (application.JobSeekerProfileId != jobSeekerProfile.JobSeekerProfileId)
                throw new Exception("You are not authorized to view this application.");

            return MapToApplicationDto(application);
        }

        public async Task WithdrawApplicationAsync(int userId, int applicationId)
        {
            var jobSeekerProfile = await _jobSeekerRepo.GetProfileByUserIdAsync(userId);
            if (jobSeekerProfile == null)
                throw new Exception("Job seeker profile not found.");

            var application = await _applicationRepo.GetByIdAsync(applicationId);
            if (application == null)
                throw new Exception("Application not found.");

            if (application.JobSeekerProfileId != jobSeekerProfile.JobSeekerProfileId)
                throw new Exception("You are not authorized to withdraw this application.");

            if (application.Status != "Applied")
                throw new Exception("Cannot withdraw application after it has been reviewed.");

            application.Status = "Withdrawn";

            await _applicationRepo.UpdateAsync(application);
            await _applicationRepo.SaveChangesAsync();
        }

        public async Task<List<ApplicantDto>> GetApplicantsByJobAsync(int userId, int jobId)
        {
            var employer = await _employerRepo.GetByUserIdAsync(userId);
            if (employer == null)
                throw new Exception("Employer profile not found.");

            var job = await _jobListingRepo.GetByIdAsync(jobId);
            if (job == null)
                throw new Exception("Job listing not found.");

            if (job.EmployerProfileId != employer.EmployerProfileId)
                throw new Exception("You are not authorized to view applications for this job.");

            var applications = await _applicationRepo.GetByJobIdAsync(jobId);
            return applications.Select(a => MapToApplicantDto(a)).ToList();
        }

        public async Task<ApplicantDto> UpdateApplicationStatusAsync(int userId, int applicationId, UpdateApplicationStatusDto dto)
        {
            var employer = await _employerRepo.GetByUserIdAsync(userId);
            if (employer == null)
                throw new Exception("Employer profile not found.");

            var application = await _applicationRepo.GetByIdAsync(applicationId);
            if (application == null)
                throw new Exception("Application not found.");

            if (application.Job.EmployerProfileId != employer.EmployerProfileId)
                throw new Exception("You are not authorized to update this application.");

            if (application.Status == "Withdrawn")
                throw new Exception("Cannot update status of a withdrawn application.");

            if (application.Status == "Rejected")
                throw new Exception("Cannot update status of a rejected application.");

            var validStatuses = new[] { "Applied", "Reviewed", "Shortlisted", "Rejected" };
            if (!validStatuses.Contains(dto.Status))
                throw new Exception("Invalid status. Valid values: Applied, Reviewed, Shortlisted, Rejected.");

            application.Status = dto.Status;

            await _applicationRepo.UpdateAsync(application);
            await _applicationRepo.SaveChangesAsync();

            try
            {
                var jobSeekerEmail = application.JobSeekerProfile.User.Email;
                var jobSeekerUserId = application.JobSeekerProfile.UserId;
                var jobTitle = application.Job.Title;

                await _emailService.SendEmailAsync(
                    jobSeekerEmail,
                    "Application Status Update - CareerCrafter",
                    $"Hi,\n\nYour application for '{jobTitle}' has been updated to: {dto.Status}.\n\nLogin to CareerCrafter to view more details.\n\n- CareerCrafter Team");

                await _notificationService.CreateNotificationAsync(jobSeekerUserId, $"Your application for '{jobTitle}' is now {dto.Status}.");
            }
            catch (Exception)
            {
                // Email/notification failure should not break status update
            }

            return MapToApplicantDto(application);
        }
    }
}
