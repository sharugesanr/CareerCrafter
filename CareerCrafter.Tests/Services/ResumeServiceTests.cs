using CareerCrafter.Core.Models;
using CareerCrafter.Repositories.Interfaces;
using CareerCrafter.Services.Implementations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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
    public class ResumeServiceTests
    {
        private Mock<IResumeRepository> _resumeRepoMock = null!;
        private Mock<IJobSeekerRepository> _jobSeekerRepoMock = null!;
        private Mock<IWebHostEnvironment> _envMock = null!;
        private ResumeService _service = null!;
        private string _testWebRootPath = null!;

        [SetUp]
        public void SetUp()
        {
            _resumeRepoMock = new Mock<IResumeRepository>();
            _jobSeekerRepoMock = new Mock<IJobSeekerRepository>();
            _envMock = new Mock<IWebHostEnvironment>();

            // use a temp folder so tests don't touch the real wwwroot
            _testWebRootPath = Path.Combine(Path.GetTempPath(), "CareerCrafterTestWebRoot");
            _envMock.Setup(e => e.WebRootPath).Returns(_testWebRootPath);

            _service = new ResumeService(_resumeRepoMock.Object, _jobSeekerRepoMock.Object, _envMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_testWebRootPath))
                Directory.Delete(_testWebRootPath, true);
        }

        private static JobSeekerProfile CreateProfile(int profileId = 1, int userId = 1)
        {
            return new JobSeekerProfile { JobSeekerProfileId = profileId, UserId = userId };
        }

        private static IFormFile CreateFakeFile(string fileName, long sizeInBytes, string content = "dummy content")
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(sizeInBytes);
            fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<System.Threading.CancellationToken>()))
                .Returns<Stream, System.Threading.CancellationToken>((target, token) => stream.CopyToAsync(target, token));

            return fileMock.Object;
        }

        [Test]
        public async Task UploadResumeAsync_ValidPdfFile_UploadsSuccessfully()
        {
            var profile = CreateProfile();
            _jobSeekerRepoMock.Setup(r => r.GetProfileByUserIdAsync(1)).ReturnsAsync(profile);
            _resumeRepoMock.Setup(r => r.AddAsync(It.IsAny<Resume>())).Returns(Task.CompletedTask);
            _resumeRepoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var file = CreateFakeFile("resume.pdf", 1024);

            var result = await _service.UploadResumeAsync(1, file);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.FileName, Is.EqualTo("resume.pdf"));
            _resumeRepoMock.Verify(r => r.AddAsync(It.IsAny<Resume>()), Times.Once);
        }

        [Test]
        public void UploadResumeAsync_InvalidExtension_ThrowsException()
        {
            var profile = CreateProfile();
            _jobSeekerRepoMock.Setup(r => r.GetProfileByUserIdAsync(1)).ReturnsAsync(profile);

            var file = CreateFakeFile("resume.exe", 1024);

            var ex = Assert.ThrowsAsync<Exception>(async () => await _service.UploadResumeAsync(1, file));
            Assert.That(ex!.Message, Is.EqualTo("Only PDF, DOC, and DOCX files are allowed."));
        }

        [Test]
        public void UploadResumeAsync_FileExceedsSizeLimit_ThrowsException()
        {
            var profile = CreateProfile();
            _jobSeekerRepoMock.Setup(r => r.GetProfileByUserIdAsync(1)).ReturnsAsync(profile);

            var file = CreateFakeFile("resume.pdf", 6 * 1024 * 1024); // 6MB > 5MB limit

            var ex = Assert.ThrowsAsync<Exception>(async () => await _service.UploadResumeAsync(1, file));
            Assert.That(ex!.Message, Is.EqualTo("File size exceeds 5MB limit."));
        }

        [Test]
        public void UploadResumeAsync_EmptyFile_ThrowsException()
        {
            var profile = CreateProfile();
            _jobSeekerRepoMock.Setup(r => r.GetProfileByUserIdAsync(1)).ReturnsAsync(profile);

            var file = CreateFakeFile("resume.pdf", 0);

            var ex = Assert.ThrowsAsync<Exception>(async () => await _service.UploadResumeAsync(1, file));
            Assert.That(ex!.Message, Is.EqualTo("Please select a file to upload."));
        }

        [Test]
        public async Task GetMyResumesAsync_ReturnsActiveResumesList()
        {
            var profile = CreateProfile();
            var resumes = new List<Resume>
            {
                new Resume { ResumeId = 1, JobSeekerProfileId = 1, FileName = "resume1.pdf", FilePath = "resumes/resume1.pdf", IsActive = true }
            };

            _jobSeekerRepoMock.Setup(r => r.GetProfileByUserIdAsync(1)).ReturnsAsync(profile);
            _resumeRepoMock.Setup(r => r.GetActiveByJobSeekerProfileIdAsync(1)).ReturnsAsync(resumes);

            var result = await _service.GetMyResumesAsync(1);

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].FileName, Is.EqualTo("resume1.pdf"));
        }

        [Test]
        public async Task GetResumeByIdAsync_OwnResume_ReturnsDto()
        {
            var profile = CreateProfile(profileId: 1, userId: 1);
            var resume = new Resume { ResumeId = 1, JobSeekerProfileId = 1, FileName = "resume.pdf", FilePath = "resumes/resume.pdf", IsActive = true };

            _jobSeekerRepoMock.Setup(r => r.GetProfileByUserIdAsync(1)).ReturnsAsync(profile);
            _resumeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(resume);

            var result = await _service.GetResumeByIdAsync(1, 1);

            Assert.That(result.FileName, Is.EqualTo("resume.pdf"));
        }

        [Test]
        public void GetResumeByIdAsync_DifferentOwner_ThrowsException()
        {
            var profile = CreateProfile(profileId: 1, userId: 1);
            var resume = new Resume { ResumeId = 1, JobSeekerProfileId = 999, FileName = "resume.pdf", FilePath = "resumes/resume.pdf", IsActive = true };

            _jobSeekerRepoMock.Setup(r => r.GetProfileByUserIdAsync(1)).ReturnsAsync(profile);
            _resumeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(resume);

            var ex = Assert.ThrowsAsync<Exception>(async () => await _service.GetResumeByIdAsync(1, 1));
            Assert.That(ex!.Message, Is.EqualTo("You are not authorized to view this resume."));
        }

        [Test]
        public async Task DeleteResumeAsync_OwnResume_SetsIsActiveFalse()
        {
            var profile = CreateProfile(profileId: 1, userId: 1);
            var resume = new Resume { ResumeId = 1, JobSeekerProfileId = 1, FileName = "resume.pdf", FilePath = "resumes/resume.pdf", IsActive = true };

            _jobSeekerRepoMock.Setup(r => r.GetProfileByUserIdAsync(1)).ReturnsAsync(profile);
            _resumeRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(resume);

            await _service.DeleteResumeAsync(1, 1);

            Assert.That(resume.IsActive, Is.False);
            _resumeRepoMock.Verify(r => r.UpdateAsync(resume), Times.Once);
        }
    }

}
