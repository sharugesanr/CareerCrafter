using CareerCrafter.Core.DTOs;
using CareerCrafter.Core.Models;
using CareerCrafter.Repositories.Interfaces;
using CareerCrafter.Services.Implementations;
using CareerCrafter.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;


namespace CareerCrafter.Tests.Services
{
    [TestFixture]
    public class AdminServiceTest
    {
        private Mock<IAdminRepository> _adminRepoMock = null!;
        private Mock<IWebHostEnvironment> _environmentMock = null!;
        private AdminService _adminService = null!;

        [SetUp]
        public void SetUp()
        {
            _adminRepoMock = new Mock<IAdminRepository>();

            _environmentMock = new Mock<IWebHostEnvironment>();

            _environmentMock
                .Setup(x => x.WebRootPath)
                .Returns(Path.GetTempPath());

            _adminService = new AdminService(
                _adminRepoMock.Object,
                _environmentMock.Object);
        }


        [Test]
        public async Task GetPlatformStatsAsync_ReturnsCorrectCounts()
        {
            
            _adminRepoMock.Setup(r => r.CountUsersByRoleAsync("JobSeeker")).ReturnsAsync(10);
            _adminRepoMock.Setup(r => r.CountUsersByRoleAsync("Employer")).ReturnsAsync(5);
            _adminRepoMock.Setup(r => r.CountActiveJobsAsync()).ReturnsAsync(12);
            _adminRepoMock.Setup(r => r.CountTotalApplicationsAsync()).ReturnsAsync(30);

            
            var result = await _adminService.GetPlatformStatsAsync();

            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.TotalJobSeekers, Is.EqualTo(10));
            Assert.That(result.TotalEmployers, Is.EqualTo(5));
            Assert.That(result.TotalActiveJobs, Is.EqualTo(12));
            Assert.That(result.TotalApplications, Is.EqualTo(30));
        }

        [Test]
        public async Task GetPlatformStatsAsync_ReturnsZeroCounts_WhenDatabaseIsEmpty()
        {
            
            _adminRepoMock.Setup(r => r.CountUsersByRoleAsync("JobSeeker")).ReturnsAsync(0);
            _adminRepoMock.Setup(r => r.CountUsersByRoleAsync("Employer")).ReturnsAsync(0);
            _adminRepoMock.Setup(r => r.CountActiveJobsAsync()).ReturnsAsync(0);
            _adminRepoMock.Setup(r => r.CountTotalApplicationsAsync()).ReturnsAsync(0);

            
            var result = await _adminService.GetPlatformStatsAsync();

            
            Assert.That(result.TotalJobSeekers, Is.EqualTo(0));
            Assert.That(result.TotalEmployers, Is.EqualTo(0));
            Assert.That(result.TotalActiveJobs, Is.EqualTo(0));
            Assert.That(result.TotalApplications, Is.EqualTo(0));
        }

        [Test]
        public async Task GetAllUsersAsync_ReturnsMappedUsers()
        {
            
            var users = new List<User>
    {
        new User
        {
            UserId = 1,
            FullName = "John Doe",
            Email = "john@test.com",
            Role = "JobSeeker",
            CreatedAt = DateTime.UtcNow
        },
        new User
        {
            UserId = 2,
            FullName = "Alice Smith",
            Email = "alice@test.com",
            Role = "Employer",
            CreatedAt = DateTime.UtcNow
        }
    };

            _adminRepoMock
                .Setup(r => r.GetAllUsersAsync())
                .ReturnsAsync(users);

            
            var result = await _adminService.GetAllUsersAsync();

            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));

            Assert.That(result[0].UserId, Is.EqualTo(1));
            Assert.That(result[0].FullName, Is.EqualTo("John Doe"));
            Assert.That(result[0].Email, Is.EqualTo("john@test.com"));
            Assert.That(result[0].Role, Is.EqualTo("JobSeeker"));

            Assert.That(result[1].UserId, Is.EqualTo(2));
            Assert.That(result[1].FullName, Is.EqualTo("Alice Smith"));
            Assert.That(result[1].Role, Is.EqualTo("Employer"));
        }

        [Test]
        public async Task GetAllUsersAsync_ReturnsEmptyList_WhenNoUsersExist()
        {
            
            _adminRepoMock
                .Setup(r => r.GetAllUsersAsync())
                .ReturnsAsync(new List<User>());

            
            var result = await _adminService.GetAllUsersAsync();

            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GetAllJobsAsync_ReturnsMappedJobs()
        {
            
            var jobs = new List<JobListing>
    {
        new JobListing
        {
            JobId = 1,
            Title = "Software Engineer",
            IsActive = true,
            PostedAt = DateTime.UtcNow,
            EmployerProfile = new EmployerProfile
            {
                CompanyName = "ABC Technologies"
            }
        },
        new JobListing
        {
            JobId = 2,
            Title = "UI Developer",
            IsActive = false,
            PostedAt = DateTime.UtcNow,
            EmployerProfile = new EmployerProfile
            {
                CompanyName = "XYZ Solutions"
            }
        }
    };

            _adminRepoMock
                .Setup(r => r.GetAllJobsAsync())
                .ReturnsAsync(jobs);

            
            var result = await _adminService.GetAllJobsAsync();

            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));

            Assert.That(result[0].JobId, Is.EqualTo(1));
            Assert.That(result[0].Title, Is.EqualTo("Software Engineer"));
            Assert.That(result[0].CompanyName, Is.EqualTo("ABC Technologies"));
            Assert.That(result[0].IsActive, Is.True);

            Assert.That(result[1].JobId, Is.EqualTo(2));
            Assert.That(result[1].Title, Is.EqualTo("UI Developer"));
            Assert.That(result[1].CompanyName, Is.EqualTo("XYZ Solutions"));
            Assert.That(result[1].IsActive, Is.False);
        }

        [Test]
        public async Task GetAllJobsAsync_ReturnsEmptyList_WhenNoJobsExist()
        {
            
            _adminRepoMock
                .Setup(r => r.GetAllJobsAsync())
                .ReturnsAsync(new List<JobListing>());

            
            var result = await _adminService.GetAllJobsAsync();

            
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task PurgeDeletedResumesAsync_NoDeletedResumes_ReturnsZero()
        {
            
            _adminRepoMock
                .Setup(r => r.GetSoftDeletedResumesAsync())
                .ReturnsAsync(new List<Resume>());

            
            var result = await _adminService.PurgeDeletedResumesAsync();

           
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Deleted, Is.EqualTo(0));
            Assert.That(result.Skipped, Is.EqualTo(0));
            Assert.That(result.Message,
                Is.EqualTo("No soft-deleted resumes found to purge."));

            _adminRepoMock.Verify(
                r => r.DeleteResumesAsync(It.IsAny<List<Resume>>()),
                Times.Never);
        }

        [Test]
        public async Task PurgeDeletedResumesAsync_ResumeLinkedToApplication_IsSkipped()
        {
            
            var resumes = new List<Resume>
    {
        new Resume
        {
            ResumeId = 1,
            FilePath = "resume.pdf",
            IsActive = false,
            Applications = new List<Application>
            {
                new Application()
            }
        }
    };

            _adminRepoMock
                .Setup(r => r.GetSoftDeletedResumesAsync())
                .ReturnsAsync(resumes);

            
            var result = await _adminService.PurgeDeletedResumesAsync();

            
            Assert.That(result.Deleted, Is.EqualTo(0));
            Assert.That(result.Skipped, Is.EqualTo(1));
            Assert.That(result.Message,
                Is.EqualTo("1 resume(s) could not be deleted because they are linked to job applications."));

            _adminRepoMock.Verify(
                r => r.DeleteResumesAsync(It.IsAny<List<Resume>>()),
                Times.Never);
        }

        [Test]
        public async Task PurgeDeletedResumesAsync_UnlinkedResume_DeletesResume()
        {
            
            var resumes = new List<Resume>
    {
        new Resume
        {
            ResumeId = 1,
            FilePath = "resume.pdf",
            IsActive = false,
            Applications = new List<Application>()
        }
    };

            _adminRepoMock
                .Setup(r => r.GetSoftDeletedResumesAsync())
                .ReturnsAsync(resumes);

            _adminRepoMock
                .Setup(r => r.DeleteResumesAsync(It.IsAny<List<Resume>>()))
                .Returns(Task.CompletedTask);

            
            var result = await _adminService.PurgeDeletedResumesAsync();

            
            Assert.That(result.Deleted, Is.EqualTo(1));
            Assert.That(result.Skipped, Is.EqualTo(0));
            Assert.That(result.Message,
                Is.EqualTo("1 resume(s) permanently deleted successfully."));

            _adminRepoMock.Verify(
                r => r.DeleteResumesAsync(It.Is<List<Resume>>(x => x.Count == 1)),
                Times.Once);
        }

        [Test]
        public async Task PurgeDeletedResumesAsync_MixedResumes_DeletesAndSkipsCorrectly()
        {
            
            var resumes = new List<Resume>
    {
        new Resume
        {
            ResumeId = 1,
            FilePath = "resume1.pdf",
            IsActive = false,
            Applications = new List<Application>()
        },
        new Resume
        {
            ResumeId = 2,
            FilePath = "resume2.pdf",
            IsActive = false,
            Applications = new List<Application>
            {
                new Application()
            }
        },
        new Resume
        {
            ResumeId = 3,
            FilePath = "resume3.pdf",
            IsActive = false,
            Applications = new List<Application>()
        }
    };

            _adminRepoMock
                .Setup(r => r.GetSoftDeletedResumesAsync())
                .ReturnsAsync(resumes);

            _adminRepoMock
                .Setup(r => r.DeleteResumesAsync(It.IsAny<List<Resume>>()))
                .Returns(Task.CompletedTask);

            
            var result = await _adminService.PurgeDeletedResumesAsync();

            
            Assert.That(result.Deleted, Is.EqualTo(2));
            Assert.That(result.Skipped, Is.EqualTo(1));
            Assert.That(result.Message,
                Is.EqualTo("2 resume(s) permanently deleted. 1 resume(s) were retained because they are linked to job applications."));

            _adminRepoMock.Verify(
                r => r.DeleteResumesAsync(It.Is<List<Resume>>(x => x.Count == 2)),
                Times.Once);
        }
    }
}