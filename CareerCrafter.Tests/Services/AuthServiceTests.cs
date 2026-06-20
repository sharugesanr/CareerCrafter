using CareerCrafter.Core.DTOs;
using CareerCrafter.Core.Models;
using CareerCrafter.Repositories.Interfaces;
using CareerCrafter.Services.Implementations;
using CareerCrafter.Services.Interfaces;
using Microsoft.Extensions.Configuration;
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
    public class AuthServiceTests
    {
        private Mock<IAuthRepository> _authRepoMock = null!;
        private Mock<IEmailService> _emailServiceMock = null!;
        private Mock<INotificationService> _notificationServiceMock = null!;
        private IConfiguration _configuration = null!;
        private AuthService _authService = null!;

        [SetUp]
        public void SetUp()
        {
            _authRepoMock = new Mock<IAuthRepository>();
            _emailServiceMock = new Mock<IEmailService>();
            _notificationServiceMock = new Mock<INotificationService>();

            var inMemorySettings = new Dictionary<string, string?>
            {
                { "JwtSettings:SecretKey", "TestSecretKeyForUnitTesting1234567890" },
                { "JwtSettings:Issuer", "CareerCrafter" },
                { "JwtSettings:Audience", "CareerCrafterUsers" },
                { "JwtSettings:ExpiryInMinutes", "60" }
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _emailServiceMock
                .Setup(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _notificationServiceMock
                .Setup(n => n.CreateNotificationAsync(It.IsAny<int>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            _authService = new AuthService(
                _authRepoMock.Object,
                _configuration,
                _emailServiceMock.Object,
                _notificationServiceMock.Object);
        }

        [Test]
        public async Task RegisterAsync_ValidJobSeeker_ReturnsAuthResponseWithToken()
        {
            var dto = new RegisterDto
            {
                FullName = "Test Seeker",
                Email = "seeker@test.com",
                Password = "Test@123",
                Role = "JobSeeker"
            };

            _authRepoMock.Setup(r => r.GetUserByEmailAsync(dto.Email))
                .ReturnsAsync((User?)null);

            _authRepoMock.Setup(r => r.AddUserAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => { u.UserId = 1; return u; });

            var result = await _authService.RegisterAsync(dto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Token, Is.Not.Null.And.Not.Empty);
            Assert.That(result.Role, Is.EqualTo("JobSeeker"));
            _authRepoMock.Verify(r => r.AddJobSeekerProfileAsync(It.IsAny<JobSeekerProfile>()), Times.Once);
        }

        [Test]
        public async Task RegisterAsync_ValidEmployer_ReturnsAuthResponseWithToken()
        {
            var dto = new RegisterDto
            {
                FullName = "Test Employer",
                Email = "employer@test.com",
                Password = "Test@123",
                Role = "Employer",
                CompanyName = "TestCorp"
            };

            _authRepoMock.Setup(r => r.GetUserByEmailAsync(dto.Email))
                .ReturnsAsync((User?)null);

            _authRepoMock.Setup(r => r.AddUserAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => { u.UserId = 2; return u; });

            var result = await _authService.RegisterAsync(dto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Role, Is.EqualTo("Employer"));
            _authRepoMock.Verify(r => r.AddEmployerProfileAsync(It.IsAny<EmployerProfile>()), Times.Once);
        }

        [Test]
        public void RegisterAsync_DuplicateEmail_ThrowsException()
        {
            var dto = new RegisterDto
            {
                FullName = "Test Seeker",
                Email = "existing@test.com",
                Password = "Test@123",
                Role = "JobSeeker"
            };

            _authRepoMock.Setup(r => r.GetUserByEmailAsync(dto.Email))
                .ReturnsAsync(new User { UserId = 1, Email = dto.Email });

            var ex = Assert.ThrowsAsync<Exception>(async () => await _authService.RegisterAsync(dto));
            Assert.That(ex!.Message, Is.EqualTo("Email already registered."));
        }

        [Test]
        public void RegisterAsync_InvalidRole_ThrowsException()
        {
            var dto = new RegisterDto
            {
                FullName = "Test User",
                Email = "invalid@test.com",
                Password = "Test@123",
                Role = "Admin"
            };

            _authRepoMock.Setup(r => r.GetUserByEmailAsync(dto.Email))
                .ReturnsAsync((User?)null);

            var ex = Assert.ThrowsAsync<Exception>(async () => await _authService.RegisterAsync(dto));
            Assert.That(ex!.Message, Is.EqualTo("Invalid role. Must be JobSeeker or Employer."));
        }

        [Test]
        public async Task LoginAsync_ValidCredentials_ReturnsAuthResponseWithToken()
        {
            var dto = new LoginDto
            {
                Email = "seeker@test.com",
                Password = "Test@123"
            };

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var existingUser = new User
            {
                UserId = 1,
                FullName = "Test Seeker",
                Email = dto.Email,
                PasswordHash = hashedPassword,
                Role = "JobSeeker"
            };

            _authRepoMock.Setup(r => r.GetUserByEmailAsync(dto.Email))
                .ReturnsAsync(existingUser);

            var result = await _authService.LoginAsync(dto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Token, Is.Not.Null.And.Not.Empty);
            Assert.That(result.Email, Is.EqualTo(dto.Email));
        }

        [Test]
        public void LoginAsync_WrongPassword_ThrowsException()
        {
            var dto = new LoginDto
            {
                Email = "seeker@test.com",
                Password = "WrongPassword"
            };

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Test@123");

            var existingUser = new User
            {
                UserId = 1,
                FullName = "Test Seeker",
                Email = dto.Email,
                PasswordHash = hashedPassword,
                Role = "JobSeeker"
            };

            _authRepoMock.Setup(r => r.GetUserByEmailAsync(dto.Email))
                .ReturnsAsync(existingUser);

            var ex = Assert.ThrowsAsync<Exception>(async () => await _authService.LoginAsync(dto));
            Assert.That(ex!.Message, Is.EqualTo("Invalid email or password."));
        }

        [Test]
        public void LoginAsync_UserNotFound_ThrowsException()
        {
            var dto = new LoginDto
            {
                Email = "doesnotexist@test.com",
                Password = "Test@123"
            };

            _authRepoMock.Setup(r => r.GetUserByEmailAsync(dto.Email))
                .ReturnsAsync((User?)null);

            var ex = Assert.ThrowsAsync<Exception>(async () => await _authService.LoginAsync(dto));
            Assert.That(ex!.Message, Is.EqualTo("Invalid email or password."));
        }
    }
}
