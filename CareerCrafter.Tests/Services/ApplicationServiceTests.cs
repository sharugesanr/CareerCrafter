using CareerCrafter.Core.DTOs;
using CareerCrafter.Core.Models;
using CareerCrafter.Repositories.Interfaces;
using CareerCrafter.Services.Implementations;
using CareerCrafter.Services.Interfaces;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CareerCrafter.Tests.Services
{
    [TestFixture]
    public class ApplicationServiceTests
    {
        private Mock<IApplicationRepository> _applicationRepoMock = null!;
        private Mock<IJobSeekerRepository> _jobSeekerRepoMock = null!;
        private Mock<IJobListingRepository> _jobListingRepoMock = null!;
        private Mock<IEmployerRepository> _employerRepoMock = null!;
        private Mock<IEmailService> _emailServiceMock = null!;
        private Mock<INotificationService> _notificationServiceMock = null!;
        private ApplicationService _service = null!;

        [SetUp]
        public void SetUp()
        {
            _applicationRepoMock = new Mock<IApplicationRepository>();
            _jobSeekerRepoMock = new Mock<IJobSeekerRepository>();
            _jobListingRepoMock = new Mock<IJobListingRepository>();
            _employerRepoMock = new Mock<IEmployerRepository>();
            _emailServiceMock = new Mock<IEmailService>();
            _notificationServiceMock = new Mock<INotificationService>();

            _emailServiceMock
                .Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _notificationServiceMock
                .Setup(n => n.CreateNotificationAsync(It.IsAny<int>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _service = new ApplicationService(
                _applicationRepoMock.Object,
                _jobSeekerRepoMock.Object,
                _jobListingRepoMock.Object,
                _employerRepoMock.Object,
                _emailServiceMock.Object,
                _notificationServiceMock.Object);
        }

        private static JobSeekerProfile CreateJobSeekerProfile(int profileId = 1, int userId = 1, int resumeId = 1)
        {
            return new JobSeekerProfile
            {
                JobSeekerProfileId = profileId,
                UserId = userId,
                Resumes = new List<Resume>
                {
                    new Resume { ResumeId = resumeId, JobSeekerProfileId = profileId, IsActive = true }
                },
                User = new User { UserId = userId, FullName = "Test Seeker", Email = "seeker@test.com" }
            };
        }

        private static JobListing CreateJob(int jobId = 1, int employerProfileId = 1, bool isActive = true)
        {
            return new JobListing
            {
                JobId = jobId,
                EmployerProfileId = employerProfileId,
                Title = "Backend Developer",
                Description = "Desc",
                IsActive = isActive,
                EmployerProfile = new EmployerProfile { EmployerProfileId = employerProfileId, CompanyName = "TestCorp" }
            };
        }

        private static Application CreateApplication(int applicationId = 1, int jobId = 1, int jobSeekerProfileId = 1, string status = "Applied")
        {
            return new Application
            {
                ApplicationId = applicationId,
                JobId = jobId,
                JobSeekerProfileId = jobSeekerProfileId,
                ResumeId = 1,
                Status = status,
                AppliedAt = DateTime.Now,
                Job = CreateJob(jobId),
                JobSeekerProfile = CreateJobSeekerProfile(jobSeekerProfileId)
            };
        }

        [Test]
        public async Task ApplyAsync_ValidData_CreatesApplicationSuccessfully()
        {
            var profile = CreateJobSeekerProfile();
            var job = CreateJob();

            _jobSeekerRepoMock.Setup(r => r.GetProfileByUserIdAsync(1)).ReturnsAsync(profile);
            _jobListingRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(job);
            _applicationRepoMock.Setup(r => r.GetByJobAndJobSeekerAsync(1, 1)).ReturnsAsync((Application?)null);
            _applicationRepoMock.Setup(r => r.AddAsync(It.IsAny<Application>())).Returns(Task.CompletedTask);
            _applicationRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
            _applicationRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(CreateApplication());

            var dto = new CreateApplicationDto { JobId = 1, ResumeId = 1, CoverNote = "Excited to apply" };

            var result = await _service.ApplyAsync(1, dto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo("Applied"));
            _applicationRepoMock.Verify(r => r.AddAsync(It.IsAny<Application>()), Times.Once);
        }

        [Test]
        public void ApplyAsync_JobInactive_ThrowsException()
        {
            var profile = CreateJobSeekerProfile();
            var job = CreateJob(isActive: false);

            _jobSeekerRepoMock.Setup(r => r.GetProfileByUserIdAsync(1)).ReturnsAsync(profile);
            _jobListingRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(job);

            var dto = new CreateApplicationDto { JobId = 1, ResumeId = 1 };

            var ex = Assert.ThrowsAsync<Exception>(async () => await _service.ApplyAsync(1, dto));
            Assert.That(ex!.Message, Is.EqualTo("Job listing not found or is no longer active."));
        }

        [Test]
        public void ApplyAsync_ResumeNotFound_ThrowsException()
        {
            var profile = CreateJobSeekerProfile(resumeId: 5); // resume id 5 exists, not 99
            var job = CreateJob();

            _jobSeekerRepoMock.Setup(r => r.GetProfileByUserIdAsync(1)).ReturnsAsync(profile);
            _jobListingRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(job);

            var dto = new CreateApplicationDto { JobId = 1, ResumeId = 99 };

            var ex = Assert.ThrowsAsync<Exception>(async () => await _service.ApplyAsync(1, dto));
            Assert.That(ex!.Message, Is.EqualTo("Resume not found. Please upload a resume before applying."));
        }

        [Test]
        public void ApplyAsync_DuplicateApplication_ThrowsException()
        {
            var profile = CreateJobSeekerProfile();
            var job = CreateJob();
            var existingApplication = CreateApplication();

            _jobSeekerRepoMock.Setup(r => r.GetProfileByUserIdAsync(1)).ReturnsAsync(profile);
            _jobListingRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(job);
            _applicationRepoMock.Setup(r => r.GetByJobAndJobSeekerAsync(1, 1)).ReturnsAsync(existingApplication);

            var dto = new CreateApplicationDto { JobId = 1, ResumeId = 1 };

            var ex = Assert.ThrowsAsync<Exception>(async () => await _service.ApplyAsync(1, dto));
            Assert.That(ex!.Message, Is.EqualTo("You have already applied for this job."));
        }

        [Test]
        public async Task GetMyApplicationsAsync_ReturnsListOfApplications()
        {
            var profile = CreateJobSeekerProfile();
            var applications = new List<Application> { CreateApplication() };

            _jobSeekerRepoMock.Setup(r => r.GetProfileByUserIdAsync(1)).ReturnsAsync(profile);
            _applicationRepoMock.Setup(r => r.GetByJobSeekerProfileIdAsync(1)).ReturnsAsync(applications);

            var result = await _service.GetMyApplicationsAsync(1);

            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task WithdrawApplicationAsync_StatusApplied_WithdrawsSuccessfully()
        {
            var profile = CreateJobSeekerProfile();
            var application = CreateApplication(status: "Applied");

            _jobSeekerRepoMock.Setup(r => r.GetProfileByUserIdAsync(1)).ReturnsAsync(profile);
            _applicationRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(application);

            await _service.WithdrawApplicationAsync(1, 1);

            Assert.That(application.Status, Is.EqualTo("Withdrawn"));
            _applicationRepoMock.Verify(r => r.UpdateAsync(application), Times.Once);
        }

        [Test]
        public void WithdrawApplicationAsync_StatusReviewed_ThrowsException()
        {
            var profile = CreateJobSeekerProfile();
            var application = CreateApplication(status: "Reviewed");

            _jobSeekerRepoMock.Setup(r => r.GetProfileByUserIdAsync(1)).ReturnsAsync(profile);
            _applicationRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(application);

            var ex = Assert.ThrowsAsync<Exception>(async () => await _service.WithdrawApplicationAsync(1, 1));
            Assert.That(ex!.Message, Is.EqualTo("Cannot withdraw application after it has been reviewed."));
        }

        [Test]
        public void WithdrawApplicationAsync_DifferentJobSeekerOwnsApplication_ThrowsException()
        {
            var profile = CreateJobSeekerProfile(profileId: 1, userId: 1);
            var application = CreateApplication(jobSeekerProfileId: 999);

            _jobSeekerRepoMock.Setup(r => r.GetProfileByUserIdAsync(1)).ReturnsAsync(profile);
            _applicationRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(application);

            var ex = Assert.ThrowsAsync<Exception>(async () => await _service.WithdrawApplicationAsync(1, 1));
            Assert.That(ex!.Message, Is.EqualTo("You are not authorized to withdraw this application."));
        }

        [Test]
        public async Task GetApplicantsByJobAsync_OwnJob_ReturnsApplicantsList()
        {
            var employer = new EmployerProfile { EmployerProfileId = 1, UserId = 1, CompanyName = "TestCorp" };
            var job = CreateJob(jobId: 1, employerProfileId: 1);
            var applications = new List<Application> { CreateApplication() };

            _employerRepoMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(employer);
            _jobListingRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(job);
            _applicationRepoMock.Setup(r => r.GetByJobIdAsync(1)).ReturnsAsync(applications);

            var result = await _service.GetApplicantsByJobAsync(1, 1);

            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetApplicantsByJobAsync_DifferentEmployerOwnsJob_ThrowsException()
        {
            var employer = new EmployerProfile { EmployerProfileId = 1, UserId = 1, CompanyName = "TestCorp" };
            var job = CreateJob(jobId: 1, employerProfileId: 999); // owned by different employer

            _employerRepoMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(employer);
            _jobListingRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(job);

            var ex = Assert.ThrowsAsync<Exception>(async () => await _service.GetApplicantsByJobAsync(1, 1));
            Assert.That(ex!.Message, Is.EqualTo("You are not authorized to view applications for this job."));
        }

        [Test]
        public async Task UpdateApplicationStatusAsync_ValidStatus_UpdatesSuccessfully()
        {
            var employer = new EmployerProfile { EmployerProfileId = 1, UserId = 1, CompanyName = "TestCorp" };
            var application = CreateApplication(status: "Applied");

            _employerRepoMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(employer);
            _applicationRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(application);

            var dto = new UpdateApplicationStatusDto { Status = "Shortlisted" };

            var result = await _service.UpdateApplicationStatusAsync(1, 1, dto);

            Assert.That(result.Status, Is.EqualTo("Shortlisted"));
            _applicationRepoMock.Verify(r => r.UpdateAsync(application), Times.Once);
        }

        [Test]
        public void UpdateApplicationStatusAsync_AlreadyWithdrawn_ThrowsException()
        {
            var employer = new EmployerProfile { EmployerProfileId = 1, UserId = 1, CompanyName = "TestCorp" };
            var application = CreateApplication(status: "Withdrawn");

            _employerRepoMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(employer);
            _applicationRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(application);

            var dto = new UpdateApplicationStatusDto { Status = "Shortlisted" };

            var ex = Assert.ThrowsAsync<Exception>(async () => await _service.UpdateApplicationStatusAsync(1, 1, dto));
            Assert.That(ex!.Message, Is.EqualTo("Cannot update status of a withdrawn application."));
        }

        [Test]
        public void UpdateApplicationStatusAsync_AlreadyRejected_ThrowsException()
        {
            var employer = new EmployerProfile { EmployerProfileId = 1, UserId = 1, CompanyName = "TestCorp" };
            var application = CreateApplication(status: "Rejected");

            _employerRepoMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(employer);
            _applicationRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(application);

            var dto = new UpdateApplicationStatusDto { Status = "Shortlisted" };

            var ex = Assert.ThrowsAsync<Exception>(async () => await _service.UpdateApplicationStatusAsync(1, 1, dto));
            Assert.That(ex!.Message, Is.EqualTo("Cannot update status of a rejected application."));
        }
    }
}
