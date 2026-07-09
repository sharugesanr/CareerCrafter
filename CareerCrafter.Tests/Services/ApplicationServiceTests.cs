using CareerCrafter.Core.DTOs;
using CareerCrafter.Core.Exceptions;
using CareerCrafter.Core.Models;
using CareerCrafter.Repositories.Interfaces;
using CareerCrafter.Services.Implementations;
using CareerCrafter.Services.Interfaces;
using Moq;
using NUnit.Framework;

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
                .Setup(x => x.SendEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _notificationServiceMock
                .Setup(x => x.CreateNotificationAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _service = new ApplicationService(
                _applicationRepoMock.Object,
                _jobSeekerRepoMock.Object,
                _jobListingRepoMock.Object,
                _employerRepoMock.Object,
                _emailServiceMock.Object,
                _notificationServiceMock.Object);
        }

        private static JobSeekerProfile CreateJobSeekerProfile(
            int profileId = 1,
            int userId = 1,
            int resumeId = 1)
        {
            return new JobSeekerProfile
            {
                JobSeekerProfileId = profileId,
                UserId = userId,

                PhoneNumber = "9876543210",
                Location = "Chennai",
                Skills = "C#, ASP.NET Core",
                Summary = "Test Summary",

                User = new User
                {
                    UserId = userId,
                    FullName = "Test Seeker",
                    Email = "seeker@test.com"
                },

                Educations = new List<Education>
                {
                    new Education
                    {
                        Degree = "B.Tech",
                        Institution = "PSNA",
                        YearOfPassing = 2026
                    }
                },

                Experiences = new List<Experience>
                {
                    new Experience
                    {
                        JobTitle = "Intern",
                        Company = "ABC Pvt Ltd",
                        Duration = "6 Months",
                        Description = "Worked on ASP.NET Core"
                    }
                },

                Resumes = new List<Resume>
                {
                    new Resume
                    {
                        ResumeId = resumeId,
                        JobSeekerProfileId = profileId,
                        FileName = "Resume.pdf",
                        UploadedAt = DateTime.Now,
                        IsActive = true
                    }
                }
            };
        }

        private static JobListing CreateJob(
            int jobId = 1,
            int employerProfileId = 1,
            bool isActive = true)
        {
            return new JobListing
            {
                JobId = jobId,
                EmployerProfileId = employerProfileId,
                Title = "Backend Developer",
                Description = "ASP.NET Core Developer",
                IsActive = isActive,

                EmployerProfile = new EmployerProfile
                {
                    EmployerProfileId = employerProfileId,
                    CompanyName = "TestCorp"
                }
            };
        }

        private static EmployerProfile CreateEmployer(
            int employerProfileId = 1,
            int userId = 1)
        {
            return new EmployerProfile
            {
                EmployerProfileId = employerProfileId,
                UserId = userId,
                CompanyName = "TestCorp"
            };
        }

        private static Application CreateApplication(
            int applicationId = 1,
            int jobId = 1,
            int jobSeekerProfileId = 1,
            string status = "Applied")
        {
            var profile = CreateJobSeekerProfile(jobSeekerProfileId);

            return new Application
            {
                ApplicationId = applicationId,
                JobId = jobId,
                JobSeekerProfileId = jobSeekerProfileId,
                ResumeId = 1,
                CoverNote = "Looking forward to joining.",
                Status = status,
                AppliedAt = DateTime.Now,

                Job = CreateJob(jobId),

                JobSeekerProfile = profile,

                Resume = profile.Resumes.First()
            };
        }


        [Test]
        public async Task ApplyAsync_ValidData_CreatesApplicationSuccessfully()
        {
            var profile = CreateJobSeekerProfile();
            var job = CreateJob();

            _jobSeekerRepoMock
                .Setup(r => r.GetProfileByUserIdAsync(1))
                .ReturnsAsync(profile);

            _jobListingRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(job);

            _applicationRepoMock
                .Setup(r => r.GetByJobAndJobSeekerAsync(1, 1))
                .ReturnsAsync((Application?)null);

            _applicationRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Application>()))
                .Returns(Task.CompletedTask);

            _applicationRepoMock
                .Setup(r => r.SaveChangesAsync())
                .Returns(Task.CompletedTask);

            _applicationRepoMock
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(CreateApplication());

            var dto = new CreateApplicationDto
            {
                JobId = 1,
                ResumeId = 1,
                CoverNote = "Interested"
            };

            var result = await _service.ApplyAsync(1, dto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Status, Is.EqualTo("Applied"));

            _applicationRepoMock.Verify(r => r.AddAsync(It.IsAny<Application>()), Times.Once);
            _applicationRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public void ApplyAsync_ProfileNotFound_ThrowsNotFoundException()
        {
            _jobSeekerRepoMock
                .Setup(r => r.GetProfileByUserIdAsync(1))
                .ReturnsAsync((JobSeekerProfile?)null);

            var dto = new CreateApplicationDto
            {
                JobId = 1,
                ResumeId = 1
            };

            var ex = Assert.ThrowsAsync<NotFoundException>(
                async () => await _service.ApplyAsync(1, dto));

            Assert.That(ex!.Message, Is.EqualTo("Job seeker profile not found."));
        }

        [Test]
        public void ApplyAsync_IncompleteProfile_ThrowsValidationException()
        {
            var profile = CreateJobSeekerProfile();
            profile.PhoneNumber = "";

            _jobSeekerRepoMock
                .Setup(r => r.GetProfileByUserIdAsync(1))
                .ReturnsAsync(profile);

            var dto = new CreateApplicationDto
            {
                JobId = 1,
                ResumeId = 1
            };

            var ex = Assert.ThrowsAsync<ValidationException>(
                async () => await _service.ApplyAsync(1, dto));

            Assert.That(ex!.Message,
                Is.EqualTo("Please complete your profile (phone number, location, skills) and add at least one education record before applying to jobs."));
        }

        [Test]
        public void ApplyAsync_NoEducation_ThrowsValidationException()
        {
            var profile = CreateJobSeekerProfile();
            profile.Educations.Clear();

            _jobSeekerRepoMock
                .Setup(r => r.GetProfileByUserIdAsync(1))
                .ReturnsAsync(profile);

            var dto = new CreateApplicationDto
            {
                JobId = 1,
                ResumeId = 1
            };

            var ex = Assert.ThrowsAsync<ValidationException>(
                async () => await _service.ApplyAsync(1, dto));

            Assert.That(ex!.Message,
                Is.EqualTo("Please complete your profile (phone number, location, skills) and add at least one education record before applying to jobs."));
        }

        [Test]
        public void ApplyAsync_JobNotFound_ThrowsNotFoundException()
        {
            var profile = CreateJobSeekerProfile();

            _jobSeekerRepoMock
                .Setup(r => r.GetProfileByUserIdAsync(1))
                .ReturnsAsync(profile);

            _jobListingRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((JobListing?)null);

            var dto = new CreateApplicationDto
            {
                JobId = 1,
                ResumeId = 1
            };

            var ex = Assert.ThrowsAsync<NotFoundException>(
                async () => await _service.ApplyAsync(1, dto));

            Assert.That(ex!.Message,
                Is.EqualTo("Job listing not found or is no longer active."));
        }

        [Test]
        public void ApplyAsync_JobInactive_ThrowsNotFoundException()
        {
            var profile = CreateJobSeekerProfile();
            var job = CreateJob(isActive: false);

            _jobSeekerRepoMock
                .Setup(r => r.GetProfileByUserIdAsync(1))
                .ReturnsAsync(profile);

            _jobListingRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(job);

            var dto = new CreateApplicationDto
            {
                JobId = 1,
                ResumeId = 1
            };

            var ex = Assert.ThrowsAsync<NotFoundException>(
                async () => await _service.ApplyAsync(1, dto));

            Assert.That(ex!.Message,
                Is.EqualTo("Job listing not found or is no longer active."));
        }

        [Test]
        public void ApplyAsync_ResumeNotFound_ThrowsNotFoundException()
        {
            var profile = CreateJobSeekerProfile(resumeId: 10);
            var job = CreateJob();

            _jobSeekerRepoMock
                .Setup(r => r.GetProfileByUserIdAsync(1))
                .ReturnsAsync(profile);

            _jobListingRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(job);

            var dto = new CreateApplicationDto
            {
                JobId = 1,
                ResumeId = 99
            };

            var ex = Assert.ThrowsAsync<NotFoundException>(
                async () => await _service.ApplyAsync(1, dto));

            Assert.That(ex!.Message,
                Is.EqualTo("Resume not found. Please upload a resume before applying."));
        }

        [Test]
        public void ApplyAsync_DuplicateApplication_ThrowsDuplicateException()
        {
            var profile = CreateJobSeekerProfile();
            var job = CreateJob();

            _jobSeekerRepoMock
                .Setup(r => r.GetProfileByUserIdAsync(1))
                .ReturnsAsync(profile);

            _jobListingRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(job);

            _applicationRepoMock
                .Setup(r => r.GetByJobAndJobSeekerAsync(1, 1))
                .ReturnsAsync(CreateApplication());

            var dto = new CreateApplicationDto
            {
                JobId = 1,
                ResumeId = 1
            };

            var ex = Assert.ThrowsAsync<DuplicateException>(
                async () => await _service.ApplyAsync(1, dto));

            Assert.That(ex!.Message,
                Is.EqualTo("You have already applied for this job."));
        }


        [Test]
        public async Task GetMyApplicationsAsync_ReturnsApplications()
        {
            var profile = CreateJobSeekerProfile();
            var applications = new List<Application>
            {
                CreateApplication(),
                CreateApplication(applicationId: 2, jobId: 2)
            };

            _jobSeekerRepoMock
                .Setup(r => r.GetProfileByUserIdAsync(1))
                .ReturnsAsync(profile);

            _applicationRepoMock
                .Setup(r => r.GetByJobSeekerProfileIdAsync(profile.JobSeekerProfileId))
                .ReturnsAsync(applications);

            var result = await _service.GetMyApplicationsAsync(1);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void GetMyApplicationsAsync_ProfileNotFound_ThrowsNotFoundException()
        {
            _jobSeekerRepoMock
                .Setup(r => r.GetProfileByUserIdAsync(1))
                .ReturnsAsync((JobSeekerProfile?)null);

            var ex = Assert.ThrowsAsync<NotFoundException>(
                async () => await _service.GetMyApplicationsAsync(1));

            Assert.That(ex!.Message,
                Is.EqualTo("Job seeker profile not found."));
        }


     
        [Test]
        public async Task GetApplicationByIdAsync_ValidApplication_ReturnsApplication()
        {
            var profile = CreateJobSeekerProfile();
            var application = CreateApplication();

            _jobSeekerRepoMock
                .Setup(r => r.GetProfileByUserIdAsync(1))
                .ReturnsAsync(profile);

            _applicationRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(application);

            var result = await _service.GetApplicationByIdAsync(1, 1);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ApplicationId, Is.EqualTo(1));
            Assert.That(result.Status, Is.EqualTo("Applied"));
        }

        [Test]
        public void GetApplicationByIdAsync_ApplicationNotFound_ThrowsNotFoundException()
        {
            var profile = CreateJobSeekerProfile();

            _jobSeekerRepoMock
                .Setup(r => r.GetProfileByUserIdAsync(1))
                .ReturnsAsync(profile);

            _applicationRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((Application?)null);

            var ex = Assert.ThrowsAsync<NotFoundException>(
                async () => await _service.GetApplicationByIdAsync(1, 1));

            Assert.That(ex!.Message,
                Is.EqualTo("Application not found."));
        }

        [Test]
        public void GetApplicationByIdAsync_UnauthorizedUser_ThrowsUnauthorizedException()
        {
            var profile = CreateJobSeekerProfile(profileId: 1);
            var application = CreateApplication(jobSeekerProfileId: 999);

            _jobSeekerRepoMock
                .Setup(r => r.GetProfileByUserIdAsync(1))
                .ReturnsAsync(profile);

            _applicationRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(application);

            var ex = Assert.ThrowsAsync<UnauthorizedException>(
                async () => await _service.GetApplicationByIdAsync(1, 1));

            Assert.That(ex!.Message,
                Is.EqualTo("You are not authorized to view applications for this job."));
        }


   

        [Test]
        public async Task WithdrawApplicationAsync_StatusApplied_WithdrawsSuccessfully()
        {
            var profile = CreateJobSeekerProfile();
            var application = CreateApplication(status: "Applied");

            _jobSeekerRepoMock
                .Setup(r => r.GetProfileByUserIdAsync(1))
                .ReturnsAsync(profile);

            _applicationRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(application);

            await _service.WithdrawApplicationAsync(1, 1);

            Assert.That(application.Status, Is.EqualTo("Withdrawn"));

            _applicationRepoMock.Verify(r => r.UpdateAsync(application), Times.Once);
            _applicationRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public void WithdrawApplicationAsync_ProfileNotFound_ThrowsNotFoundException()
        {
            _jobSeekerRepoMock
                .Setup(r => r.GetProfileByUserIdAsync(1))
                .ReturnsAsync((JobSeekerProfile?)null);

            var ex = Assert.ThrowsAsync<NotFoundException>(
                async () => await _service.WithdrawApplicationAsync(1, 1));

            Assert.That(ex!.Message,
                Is.EqualTo("Job seeker profile not found."));
        }

        [Test]
        public void WithdrawApplicationAsync_ApplicationNotFound_ThrowsNotFoundException()
        {
            var profile = CreateJobSeekerProfile();

            _jobSeekerRepoMock
                .Setup(r => r.GetProfileByUserIdAsync(1))
                .ReturnsAsync(profile);

            _applicationRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((Application?)null);

            var ex = Assert.ThrowsAsync<NotFoundException>(
                async () => await _service.WithdrawApplicationAsync(1, 1));

            Assert.That(ex!.Message,
                Is.EqualTo("Application not found."));
        }

        [Test]
        public void WithdrawApplicationAsync_AlreadyWithdrawn_ThrowsValidationException()
        {
            var profile = CreateJobSeekerProfile();
            var application = CreateApplication(status: "Withdrawn");

            _jobSeekerRepoMock
                .Setup(r => r.GetProfileByUserIdAsync(1))
                .ReturnsAsync(profile);

            _applicationRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(application);

            var ex = Assert.ThrowsAsync<ValidationException>(
                async () => await _service.WithdrawApplicationAsync(1, 1));

            Assert.That(ex!.Message,
                Is.EqualTo("You have already Withdrawed this Application."));
        }

        [Test]
        public void WithdrawApplicationAsync_StatusReviewed_ThrowsValidationException()
        {
            var profile = CreateJobSeekerProfile();
            var application = CreateApplication(status: "Reviewed");

            _jobSeekerRepoMock
                .Setup(r => r.GetProfileByUserIdAsync(1))
                .ReturnsAsync(profile);

            _applicationRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(application);

            var ex = Assert.ThrowsAsync<ValidationException>(
                async () => await _service.WithdrawApplicationAsync(1, 1));

            Assert.That(ex!.Message,
                Is.EqualTo("Cannot withdraw application after it has been reviewed."));
        }

        [Test]
        public void WithdrawApplicationAsync_UnauthorizedUser_ThrowsUnauthorizedException()
        {
            var profile = CreateJobSeekerProfile(profileId: 1);
            var application = CreateApplication(jobSeekerProfileId: 999);

            _jobSeekerRepoMock
                .Setup(r => r.GetProfileByUserIdAsync(1))
                .ReturnsAsync(profile);

            _applicationRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(application);

            var ex = Assert.ThrowsAsync<UnauthorizedException>(
                async () => await _service.WithdrawApplicationAsync(1, 1));

            Assert.That(ex!.Message,
                Is.EqualTo("You are not authorized to withdraw this application."));
        }

        

        [Test]
        public async Task GetApplicantsByJobAsync_OwnJob_ReturnsApplicantsList()
        {
            var employer = CreateEmployer();
            var job = CreateJob();
            var applications = new List<Application>
            {
                CreateApplication()
            };

            _employerRepoMock
                .Setup(r => r.GetByUserIdAsync(1))
                .ReturnsAsync(employer);

            _jobListingRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(job);

            _applicationRepoMock
                .Setup(r => r.GetByJobIdAsync(1))
                .ReturnsAsync(applications);

            var result = await _service.GetApplicantsByJobAsync(1, 1);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void GetApplicantsByJobAsync_EmployerNotFound_ThrowsNotFoundException()
        {
            _employerRepoMock
                .Setup(r => r.GetByUserIdAsync(1))
                .ReturnsAsync((EmployerProfile?)null);

            var ex = Assert.ThrowsAsync<NotFoundException>(
                async () => await _service.GetApplicantsByJobAsync(1, 1));

            Assert.That(ex!.Message,
                Is.EqualTo("Employer profile not found."));
        }

        [Test]
        public void GetApplicantsByJobAsync_JobNotFound_ThrowsNotFoundException()
        {
            var employer = CreateEmployer();

            _employerRepoMock
                .Setup(r => r.GetByUserIdAsync(1))
                .ReturnsAsync(employer);

            _jobListingRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((JobListing?)null);

            var ex = Assert.ThrowsAsync<NotFoundException>(
                async () => await _service.GetApplicantsByJobAsync(1, 1));

            Assert.That(ex!.Message,
                Is.EqualTo("Job listing not found."));
        }

        [Test]
        public void GetApplicantsByJobAsync_UnauthorizedEmployer_ThrowsUnauthorizedException()
        {
            var employer = CreateEmployer(employerProfileId: 1);
            var job = CreateJob(employerProfileId: 999);

            _employerRepoMock
                .Setup(r => r.GetByUserIdAsync(1))
                .ReturnsAsync(employer);

            _jobListingRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(job);

            var ex = Assert.ThrowsAsync<UnauthorizedException>(
                async () => await _service.GetApplicantsByJobAsync(1, 1));

            Assert.That(ex!.Message,
                Is.EqualTo("You are not authorized to view applications for this job."));
        }



        [Test]
        public async Task UpdateApplicationStatusAsync_ValidStatus_UpdatesSuccessfully()
        {
            var employer = CreateEmployer();
            var application = CreateApplication(status: "Applied");

            _employerRepoMock
                .Setup(r => r.GetByUserIdAsync(1))
                .ReturnsAsync(employer);

            _applicationRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(application);

            var dto = new UpdateApplicationStatusDto
            {
                Status = "Shortlisted"
            };

            var result = await _service.UpdateApplicationStatusAsync(1, 1, dto);

            Assert.That(result.Status, Is.EqualTo("Shortlisted"));

            _applicationRepoMock.Verify(r => r.UpdateAsync(application), Times.Once);
            _applicationRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);

            _emailServiceMock.Verify(e =>
                e.SendEmailAsync(
                    application.JobSeekerProfile.User.Email,
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Once);

            _notificationServiceMock.Verify(n =>
                n.CreateNotificationAsync(
                    application.JobSeekerProfile.UserId,
                    It.IsAny<string>()),
                Times.Once);
        }

        [Test]
        public void UpdateApplicationStatusAsync_InvalidStatus_ThrowsValidationException()
        {
            var employer = CreateEmployer();
            var application = CreateApplication(status: "Applied");

            _employerRepoMock
                .Setup(r => r.GetByUserIdAsync(1))
                .ReturnsAsync(employer);

            _applicationRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(application);

            var dto = new UpdateApplicationStatusDto
            {
                Status = "Completed"
            };

            var ex = Assert.ThrowsAsync<ValidationException>(
                async () => await _service.UpdateApplicationStatusAsync(1, 1, dto));

            Assert.That(ex!.Message,
                Is.EqualTo("Invalid status. Valid values: Applied, Reviewed, Shortlisted, Rejected."));
        }

        [Test]
        public void UpdateApplicationStatusAsync_WithdrawnApplication_ThrowsValidationException()
        {
            var employer = CreateEmployer();
            var application = CreateApplication(status: "Withdrawn");

            _employerRepoMock
                .Setup(r => r.GetByUserIdAsync(1))
                .ReturnsAsync(employer);

            _applicationRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(application);

            var dto = new UpdateApplicationStatusDto
            {
                Status = "Reviewed"
            };

            var ex = Assert.ThrowsAsync<ValidationException>(
                async () => await _service.UpdateApplicationStatusAsync(1, 1, dto));

            Assert.That(ex!.Message,
                Is.EqualTo("Cannot update status of a withdrawn application."));
        }

        [Test]
        public void UpdateApplicationStatusAsync_RejectedApplication_ThrowsValidationException()
        {
            var employer = CreateEmployer();
            var application = CreateApplication(status: "Rejected");

            _employerRepoMock
                .Setup(r => r.GetByUserIdAsync(1))
                .ReturnsAsync(employer);

            _applicationRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(application);

            var dto = new UpdateApplicationStatusDto
            {
                Status = "Reviewed"
            };

            var ex = Assert.ThrowsAsync<ValidationException>(
                async () => await _service.UpdateApplicationStatusAsync(1, 1, dto));

            Assert.That(ex!.Message,
                Is.EqualTo("Cannot update status of a rejected application."));
        }

        

        [Test]
        public async Task UpdateApplicationStatusAsync_EmailFails_StatusStillUpdated()
        {
            var employer = CreateEmployer();
            var application = CreateApplication(status: "Applied");

            _employerRepoMock
                .Setup(r => r.GetByUserIdAsync(1))
                .ReturnsAsync(employer);

            _applicationRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(application);

            _emailServiceMock
                .Setup(e => e.SendEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .ThrowsAsync(new Exception("SMTP Error"));

            var dto = new UpdateApplicationStatusDto
            {
                Status = "Reviewed"
            };

            var result = await _service.UpdateApplicationStatusAsync(1, 1, dto);

            Assert.That(result.Status, Is.EqualTo("Reviewed"));

            _applicationRepoMock.Verify(r => r.UpdateAsync(application), Times.Once);
            _applicationRepoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }



        [Test]
        public async Task GetCandidateProfileAsync_ValidApplication_ReturnsCandidateProfile()
        {
            var employer = CreateEmployer();
            var application = CreateApplication();
            var profile = CreateJobSeekerProfile();

            _employerRepoMock
                .Setup(r => r.GetByUserIdAsync(1))
                .ReturnsAsync(employer);

            _applicationRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(application);

            _jobSeekerRepoMock
                .Setup(r => r.GetProfileByUserIdAsync(profile.UserId))
                .ReturnsAsync(profile);

            var result = await _service.GetCandidateProfileAsync(1, 1);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.FullName, Is.EqualTo(profile.User.FullName));
            Assert.That(result.Email, Is.EqualTo(profile.User.Email));
            Assert.That(result.PhoneNumber, Is.EqualTo(profile.PhoneNumber));
            Assert.That(result.ResumeFileName, Is.EqualTo("Resume.pdf"));
        }

        [Test]
        public void GetCandidateProfileAsync_EmployerNotFound_ThrowsNotFoundException()
        {
            _employerRepoMock
                .Setup(r => r.GetByUserIdAsync(1))
                .ReturnsAsync((EmployerProfile?)null);

            var ex = Assert.ThrowsAsync<NotFoundException>(
                async () => await _service.GetCandidateProfileAsync(1, 1));

            Assert.That(ex!.Message,
                Is.EqualTo("Employer profile not found."));
        }

        [Test]
        public void GetCandidateProfileAsync_ApplicationNotFound_ThrowsNotFoundException()
        {
            var employer = CreateEmployer();

            _employerRepoMock
                .Setup(r => r.GetByUserIdAsync(1))
                .ReturnsAsync(employer);

            _applicationRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync((Application?)null);

            var ex = Assert.ThrowsAsync<NotFoundException>(
                async () => await _service.GetCandidateProfileAsync(1, 1));

            Assert.That(ex!.Message,
                Is.EqualTo("Application not found."));
        }

        [Test]
        public void GetCandidateProfileAsync_UnauthorizedEmployer_ThrowsUnauthorizedException()
        {
            var employer = CreateEmployer(employerProfileId: 1);
            var application = CreateApplication();

            application.Job.EmployerProfileId = 999;

            _employerRepoMock
                .Setup(r => r.GetByUserIdAsync(1))
                .ReturnsAsync(employer);

            _applicationRepoMock
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(application);

            var ex = Assert.ThrowsAsync<UnauthorizedException>(
                async () => await _service.GetCandidateProfileAsync(1, 1));

            Assert.That(ex!.Message,
                Is.EqualTo("You are not authorized to view this candidate's profile."));
        }
    }
}