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
    public class EmployerServiceTests
    {
        private Mock<IEmployerRepository> _repoMock = null!;
        private EmployerService _service = null!;

        [SetUp]
        public void SetUp()
        {
            _repoMock = new Mock<IEmployerRepository>();
            _service = new EmployerService(_repoMock.Object);
        }

        private static EmployerProfile CreateProfile(int profileId = 1, int userId = 1)
        {
            return new EmployerProfile
            {
                EmployerProfileId = profileId,
                UserId = userId,
                CompanyName = "TestCorp",
                Industry = "IT",
                Website = "https://testcorp.com",
                Location = "Chennai",
                Description = "A test company",
                User = new User
                {
                    UserId = userId,
                    FullName = "Test Employer",
                    Email = "employer@test.com"
                }
            };
        }

        [Test]
        public async Task GetProfileAsync_ProfileExists_ReturnsDto()
        {
            var profile = CreateProfile();
            _repoMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(profile);

            var result = await _service.GetProfileAsync(1);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.CompanyName, Is.EqualTo("TestCorp"));
            Assert.That(result.FullName, Is.EqualTo("Test Employer"));
        }

        [Test]
        public void GetProfileAsync_ProfileNotFound_ThrowsException()
        {
            _repoMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync((EmployerProfile?)null);

            var ex = Assert.ThrowsAsync<Exception>(async () => await _service.GetProfileAsync(1));
            Assert.That(ex!.Message, Is.EqualTo("Profile not found."));
        }

        [Test]
        public async Task UpdateProfileAsync_ValidData_UpdatesAndReturnsDto()
        {
            var profile = CreateProfile();
            _repoMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(profile);

            var dto = new UpdateEmployerProfileDto
            {
                CompanyName = "UpdatedCorp",
                Industry = "Finance",
                Website = "https://updatedcorp.com",
                Location = "Bangalore",
                Description = "Updated description"
            };

            var result = await _service.UpdateProfileAsync(1, dto);

            Assert.That(result.CompanyName, Is.EqualTo("UpdatedCorp"));
            Assert.That(result.Location, Is.EqualTo("Bangalore"));
            _repoMock.Verify(r => r.UpdateProfileAsync(profile), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public void UpdateProfileAsync_ProfileNotFound_ThrowsException()
        {
            _repoMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync((EmployerProfile?)null);

            var dto = new UpdateEmployerProfileDto
            {
                CompanyName = "UpdatedCorp",
                Industry = "Finance",
                Website = "https://updatedcorp.com",
                Location = "Bangalore",
                Description = "Updated description"
            };

            var ex = Assert.ThrowsAsync<Exception>(async () => await _service.UpdateProfileAsync(1, dto));
            Assert.That(ex!.Message, Is.EqualTo("Profile not found."));
        }

        [Test]
        public async Task UpdateProfileAsync_PreservesUserDetailsAfterUpdate()
        {
            var profile = CreateProfile();
            _repoMock.Setup(r => r.GetByUserIdAsync(1)).ReturnsAsync(profile);

            var dto = new UpdateEmployerProfileDto
            {
                CompanyName = "NewName",
                Industry = "IT",
                Website = "https://newname.com",
                Location = "Chennai",
                Description = "New description"
            };

            var result = await _service.UpdateProfileAsync(1, dto);

            // Confirm linked User info remains intact even after profile field updates
            Assert.That(result.Email, Is.EqualTo("employer@test.com"));
            Assert.That(result.FullName, Is.EqualTo("Test Employer"));
        }
    }
}
