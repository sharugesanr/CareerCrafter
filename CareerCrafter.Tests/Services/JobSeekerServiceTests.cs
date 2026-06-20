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
    public class JobSeekerServiceTests
    {
        private Mock<IJobSeekerRepository> _repoMock = null!;
        private JobSeekerService _service = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IJobSeekerRepository>();
            _service = new JobSeekerService(_repoMock.Object);
        }

        private static JobSeekerProfile CreateProfile(int profileId = 1, int userId = 1)
        {
            return new JobSeekerProfile
            {
                JobSeekerProfileId = profileId,
                UserId = userId,
                PhoneNumber = "9999999999",
                Location = "Chennai",
                Summary = "Test summary",
                Skills = "C#, React",
                User = new User
                {
                    UserId = userId,
                    FullName = "Test Seeker",
                    Email = "seeker@test.com"
                }
            };
        }

        [Test]
        public async Task GetProfileAsync_ProfileExists_ReturnsDto()
        {
            var profile = CreateProfile();
            _repoMock.Setup(r => r.GetProfileByUserIdAsync(1)).ReturnsAsync(profile);

            var result = await _service.GetProfileAsync(1);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.FullName, Is.EqualTo("Test Seeker"));
            Assert.That(result.Skills, Is.EqualTo("C#, React"));
        }

        [Test]
        public void GetProfileAsync_ProfileNotFound_ThrowsException()
        {
            _repoMock.Setup(r => r.GetProfileByUserIdAsync(1)).ReturnsAsync((JobSeekerProfile?)null);

            var ex = Assert.ThrowsAsync<Exception>(async () => await _service.GetProfileAsync(1));
            Assert.That(ex!.Message, Is.EqualTo("Profile not found."));
        }

        [Test]
        public async Task UpdateProfileAsync_ValidData_UpdatesAndReturnsDto()
        {
            var profile = CreateProfile();
            _repoMock.Setup(r => r.GetProfileByUserIdAsync(1)).ReturnsAsync(profile);

            var dto = new UpdateJobSeekerProfileDto
            {
                PhoneNumber = "8888888888",
                Location = "Bangalore",
                Summary = "Updated summary",
                Skills = "C#, React, SQL"
            };

            var result = await _service.UpdateProfileAsync(1, dto);

            Assert.That(result.Location, Is.EqualTo("Bangalore"));
            Assert.That(result.Skills, Is.EqualTo("C#, React, SQL"));
            _repoMock.Verify(r => r.UpdateProfileAsync(profile), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task AddEducationAsync_ValidData_ReturnsEducationDto()
        {
            var profile = CreateProfile();
            _repoMock.Setup(r => r.GetProfileByUserIdAsync(1)).ReturnsAsync(profile);

            var dto = new AddEducationDto
            {
                Degree = "B.Tech",
                Institution = "PSNACET",
                YearOfPassing = 2026
            };

            var result = await _service.AddEducationAsync(1, dto);

            Assert.That(result.Degree, Is.EqualTo("B.Tech"));
            Assert.That(result.Institution, Is.EqualTo("PSNACET"));
            _repoMock.Verify(r => r.AddEducationAsync(It.IsAny<Education>()), Times.Once);
        }

        [Test]
        public void AddEducationAsync_ProfileNotFound_ThrowsException()
        {
            _repoMock.Setup(r => r.GetProfileByUserIdAsync(1)).ReturnsAsync((JobSeekerProfile?)null);

            var dto = new AddEducationDto { Degree = "B.Tech", Institution = "PSNACET", YearOfPassing = 2026 };

            var ex = Assert.ThrowsAsync<Exception>(async () => await _service.AddEducationAsync(1, dto));
            Assert.That(ex!.Message, Is.EqualTo("Profile not found."));
        }

        [Test]
        public async Task DeleteEducationAsync_ValidOwnership_DeletesSuccessfully()
        {
            var profile = CreateProfile();
            var education = new Education { EducationId = 10, JobSeekerProfileId = profile.JobSeekerProfileId };

            _repoMock.Setup(r => r.GetProfileByUserIdAsync(1)).ReturnsAsync(profile);
            _repoMock.Setup(r => r.GetEducationByIdAsync(10)).ReturnsAsync(education);

            await _service.DeleteEducationAsync(1, 10);

            _repoMock.Verify(r => r.DeleteEducationAsync(education), Times.Once);
        }

        [Test]
        public void DeleteEducationAsync_BelongsToDifferentProfile_ThrowsException()
        {
            var profile = CreateProfile(profileId: 1);
            var education = new Education { EducationId = 10, JobSeekerProfileId = 999 }; // different profile

            _repoMock.Setup(r => r.GetProfileByUserIdAsync(1)).ReturnsAsync(profile);
            _repoMock.Setup(r => r.GetEducationByIdAsync(10)).ReturnsAsync(education);

            var ex = Assert.ThrowsAsync<Exception>(async () => await _service.DeleteEducationAsync(1, 10));
            Assert.That(ex!.Message, Is.EqualTo("Education record not found."));
        }

        [Test]
        public async Task AddExperienceAsync_ValidData_ReturnsExperienceDto()
        {
            var profile = CreateProfile();
            _repoMock.Setup(r => r.GetProfileByUserIdAsync(1)).ReturnsAsync(profile);

            var dto = new AddExperienceDto
            {
                JobTitle = "Software Engineer",
                Company = "TestCorp",
                Duration = "2 years",
                Description = "Backend development"
            };

            var result = await _service.AddExperienceAsync(1, dto);

            Assert.That(result.JobTitle, Is.EqualTo("Software Engineer"));
            Assert.That(result.Company, Is.EqualTo("TestCorp"));
            _repoMock.Verify(r => r.AddExperienceAsync(It.IsAny<Experience>()), Times.Once);
        }

        [Test]
        public async Task DeleteExperienceAsync_ValidOwnership_DeletesSuccessfully()
        {
            var profile = CreateProfile();
            var experience = new Experience { ExperienceId = 5, JobSeekerProfileId = profile.JobSeekerProfileId };

            _repoMock.Setup(r => r.GetProfileByUserIdAsync(1)).ReturnsAsync(profile);
            _repoMock.Setup(r => r.GetExperienceByIdAsync(5)).ReturnsAsync(experience);

            await _service.DeleteExperienceAsync(1, 5);

            _repoMock.Verify(r => r.DeleteExperienceAsync(experience), Times.Once);
        }
    }
}
