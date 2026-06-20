using CareerCrafter.Core.DTOs;
using CareerCrafter.Core.Models;
using CareerCrafter.Repositories.Interfaces;
using CareerCrafter.Services.Implementations;
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
    public class JobListingServiceTests
    {
        private Mock<IJobListingRepository> _jobRepoMock = null!;
        private Mock<IEmployerRepository> _employerRepoMock = null!;
        private JobListingService _service = null!;

        [SetUp]
        public void SetUp()
        {
            _jobRepoMock = new Mock<IJobListingRepository>();
            _employerRepoMock = new Mock<IEmployerRepository>();
            _service = new JobListingService(_jobRepoMock.Object, _employerRepoMock.Object);
        }

        private static EmployerProfile CreateEmployer(int profileId = 1, int userId = 1)
        {
            return new EmployerProfile
            {
                EmployerProfileId = profileId,
                UserId = userId,
                CompanyName = "TestCorp",
                Industry = "IT"
            };
        }

        private static JobListing CreateJob(int jobId = 1, int employerProfileId = 1, bool isActive = true, string title = "Backend Developer")
        {
            return new JobListing
            {
                JobId = jobId,
                EmployerProfileId = employerProfileId,
                Title = title,
                Description = "Job description",
                Location = "Chennai",
                JobType = "FullTime",
                IsActive = isActive,
                PostedAt = DateTime.Now,
                EmployerProfile = CreateEmployer(employerProfileId)
            };
        }

        [Test]
        public async Task CreateJobAsync_ValidEmployer_ReturnsJobDto()
        {
            var employer = CreateEmployer();
            var dto = new CreateJobListingDto
            {
                Title = "Backend Developer",
                Description = "Job description",
                Location = "Chennai",
                JobType = "FullTime"
            };

            _employerRepoMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(employer);
            _jobRepoMock.Setup(r => r.AddAsync(It.IsAny<JobListing>())).Returns(Task.CompletedTask);
            _jobRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
            _jobRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(CreateJob());

            var result = await _service.CreateJobAsync(1, dto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Title, Is.EqualTo("Backend Developer"));
            _jobRepoMock.Verify(r => r.AddAsync(It.IsAny<JobListing>()), Times.Once);
        }

        [Test]
        public void CreateJobAsync_EmployerNotFound_ThrowsException()
        {
            _employerRepoMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync((EmployerProfile?)null);

            var dto = new CreateJobListingDto { Title = "Backend Developer", Description = "Desc" };

            var ex = Assert.ThrowsAsync<Exception>(async () => await _service.CreateJobAsync(1, dto));
            Assert.That(ex!.Message, Is.EqualTo("Employer profile not found."));
        }

        [Test]
        public async Task UpdateJobAsync_OwnJob_UpdatesSuccessfully()
        {
            var employer = CreateEmployer(profileId: 1, userId: 1);
            var job = CreateJob(jobId: 5, employerProfileId: 1);

            _employerRepoMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(employer);
            _jobRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(job);

            var dto = new UpdateJobListingDto
            {
                Title = "Senior Backend Developer",
                Description = "Updated description",
                Location = "Bangalore"
            };

            var result = await _service.UpdateJobAsync(1, 5, dto);

            Assert.That(result.Title, Is.EqualTo("Senior Backend Developer"));
            _jobRepoMock.Verify(r => r.UpdateAsync(job), Times.Once);
        }

        [Test]
        public void UpdateJobAsync_JobNotFound_ThrowsException()
        {
            var employer = CreateEmployer();
            _employerRepoMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(employer);
            _jobRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((JobListing?)null);

            var dto = new UpdateJobListingDto { Title = "Test", Description = "Test" };

            var ex = Assert.ThrowsAsync<Exception>(async () => await _service.UpdateJobAsync(1, 99, dto));
            Assert.That(ex!.Message, Is.EqualTo("Job listing not found."));
        }

        [Test]
        public void UpdateJobAsync_DifferentEmployerOwnsJob_ThrowsUnauthorizedException()
        {
            var employer = CreateEmployer(profileId: 1, userId: 1);
            var job = CreateJob(jobId: 5, employerProfileId: 999); // belongs to a different employer

            _employerRepoMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(employer);
            _jobRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(job);

            var dto = new UpdateJobListingDto { Title = "Test", Description = "Test" };

            var ex = Assert.ThrowsAsync<Exception>(async () => await _service.UpdateJobAsync(1, 5, dto));
            Assert.That(ex!.Message, Is.EqualTo("You are not authorized to update this job."));
        }

        [Test]
        public async Task SoftDeleteJobAsync_OwnJob_SetsIsActiveFalse()
        {
            var employer = CreateEmployer(profileId: 1, userId: 1);
            var job = CreateJob(jobId: 5, employerProfileId: 1, isActive: true);

            _employerRepoMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(employer);
            _jobRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(job);

            await _service.SoftDeleteJobAsync(1, 5);

            Assert.That(job.IsActive, Is.False);
            _jobRepoMock.Verify(r => r.UpdateAsync(job), Times.Once);
        }

        [Test]
        public void SoftDeleteJobAsync_DifferentEmployerOwnsJob_ThrowsException()
        {
            var employer = CreateEmployer(profileId: 1, userId: 1);
            var job = CreateJob(jobId: 5, employerProfileId: 999);

            _employerRepoMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(employer);
            _jobRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(job);

            var ex = Assert.ThrowsAsync<Exception>(async () => await _service.SoftDeleteJobAsync(1, 5));
            Assert.That(ex!.Message, Is.EqualTo("You are not authorized to delete this job."));
        }

        [Test]
        public async Task ReactivateJobAsync_OwnJob_SetsIsActiveTrue()
        {
            var employer = CreateEmployer(profileId: 1, userId: 1);
            var job = CreateJob(jobId: 5, employerProfileId: 1, isActive: false);

            _employerRepoMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(employer);
            _jobRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(job);

            await _service.ReactivateJobAsync(1, 5);

            Assert.That(job.IsActive, Is.True);
            _jobRepoMock.Verify(r => r.UpdateAsync(job), Times.Once);
        }

        [Test]
        public async Task GetJobByIdAsync_ActiveJob_ReturnsDto()
        {
            var job = CreateJob(jobId: 5, isActive: true);
            _jobRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(job);

            var result = await _service.GetJobByIdAsync(5);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.JobId, Is.EqualTo(5));
        }

        [Test]
        public async Task GetJobByIdAsync_InactiveJob_ReturnsNull()
        {
            var job = CreateJob(jobId: 5, isActive: false);
            _jobRepoMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(job);

            var result = await _service.GetJobByIdAsync(5);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task SearchJobsAsync_FilterByTitle_ReturnsMatchingJobsOnly()
        {
            var jobs = new List<JobListing>
            {
                CreateJob(jobId: 1, title: "Backend Developer"),
                CreateJob(jobId: 2, title: "Frontend Developer"),
                CreateJob(jobId: 3, title: "Backend Lead")
            };

            _jobRepoMock.Setup(r => r.GetAllActiveAsync()).ReturnsAsync(jobs);

            var searchDto = new JobSearchDto { Title = "Backend", Page = 1, PageSize = 10 };

            var result = await _service.SearchJobsAsync(searchDto);

            Assert.That(result.TotalCount, Is.EqualTo(2));
            Assert.That(result.Items.All(j => j.Title.Contains("Backend")), Is.True);
        }

        [Test]
        public async Task SearchJobsAsync_Pagination_ReturnsCorrectPageSize()
        {
            var jobs = Enumerable.Range(1, 15)
                .Select(i => CreateJob(jobId: i, title: $"Job {i}"))
                .ToList();

            _jobRepoMock.Setup(r => r.GetAllActiveAsync()).ReturnsAsync(jobs);

            var searchDto = new JobSearchDto { Page = 1, PageSize = 10 };

            var result = await _service.SearchJobsAsync(searchDto);

            Assert.That(result.Items.Count, Is.EqualTo(10));
            Assert.That(result.TotalCount, Is.EqualTo(15));
            Assert.That(result.TotalPages, Is.EqualTo(2));
        }
    }
}
