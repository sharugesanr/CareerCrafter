using CareerCrafter.Core.DTOs;
using CareerCrafter.Core.Exceptions;
using CareerCrafter.Core.Models;
using CareerCrafter.Repositories.Interfaces;
using CareerCrafter.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CareerCrafter.Services.Implementations
{
    public class ResumeService : IResumeService
    {
        private readonly IResumeRepository _resumeRepo;
        private readonly IJobSeekerRepository _jobSeekerRepo;
        private readonly string _uploadFolder;

        private static readonly string[] AllowedExtensions = { ".pdf", ".doc", ".docx" };
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB

        public ResumeService(IResumeRepository resumeRepo, IJobSeekerRepository jobSeekerRepo, IWebHostEnvironment environment)
        {
            _resumeRepo = resumeRepo;
            _jobSeekerRepo = jobSeekerRepo;
            _uploadFolder = Path.Combine(environment.WebRootPath, "resumes");

            if (!Directory.Exists(_uploadFolder))
                Directory.CreateDirectory(_uploadFolder);
        }

        private static ResumeDto MapToDto(Resume resume)
        {
            return new ResumeDto
            {
                ResumeId = resume.ResumeId,
                FileName = resume.FileName,
                FilePath = resume.FilePath,
                UploadedAt = resume.UploadedAt
            };
        }

        public async Task<ResumeDto> UploadResumeAsync(int userId, IFormFile file)
        {
            var jobSeekerProfile = await _jobSeekerRepo.GetProfileByUserIdAsync(userId);
            if (jobSeekerProfile == null)
                throw new NotFoundException("Job seeker profile not found.");

            var existingResumes = await _resumeRepo.GetActiveByJobSeekerProfileIdAsync(jobSeekerProfile.JobSeekerProfileId);
            if (existingResumes.Count >= 4)
                throw new ValidationException("Resume limit reached. You can store a maximum of 4 resumes. Please delete an existing resume before uploading a new one.");

            if (file == null || file.Length == 0)
                throw new ValidationException("Please select a file to upload.");

            if (file.Length > MaxFileSizeBytes)
                throw new ValidationException("File size exceeds 5MB limit.");


            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                throw new ValidationException("Only PDF, DOC, and DOCX files are allowed.");


            var uniqueFileName = $"{userId}_{DateTime.Now:yyyyMMddHHmmss}_{Path.GetFileName(file.FileName)}";
            var filePath = Path.Combine(_uploadFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var resume = new Resume
            {
                JobSeekerProfileId = jobSeekerProfile.JobSeekerProfileId,
                FileName = file.FileName,
                FilePath = $"resumes/{uniqueFileName}",
                UploadedAt = DateTime.Now,
                IsActive = true
            };

            await _resumeRepo.AddAsync(resume);
            await _resumeRepo.SaveChangesAsync();

            return MapToDto(resume);
        }

        public async Task<List<ResumeDto>> GetMyResumesAsync(int userId)
        {
            var jobSeekerProfile = await _jobSeekerRepo.GetProfileByUserIdAsync(userId);
            if (jobSeekerProfile == null)
                throw new NotFoundException("Job seeker profile not found.");

            var resumes = await _resumeRepo.GetActiveByJobSeekerProfileIdAsync(jobSeekerProfile.JobSeekerProfileId);
            return resumes.Select(r => MapToDto(r)).ToList();
        }

        public async Task<ResumeDto> GetResumeByIdAsync(int userId, int resumeId)
        {
            var jobSeekerProfile = await _jobSeekerRepo.GetProfileByUserIdAsync(userId);
            if (jobSeekerProfile == null)
                throw new NotFoundException("Job seeker profile not found.");

            var resume = await _resumeRepo.GetByIdAsync(resumeId);
            if (resume == null || resume.IsActive == false)
                throw new NotFoundException("Resume not found.");

            if (resume.JobSeekerProfileId != jobSeekerProfile.JobSeekerProfileId)
                throw new UnauthorizedException("You are not authorized to view this resume.");

            return MapToDto(resume);
        }

        public async Task DeleteResumeAsync(int userId, int resumeId)
        {
            var jobSeekerProfile = await _jobSeekerRepo.GetProfileByUserIdAsync(userId);
            if (jobSeekerProfile == null)
                throw new NotFoundException("Job seeker profile not found.");

            var resume = await _resumeRepo.GetByIdAsync(resumeId);
            if (resume == null || resume.IsActive == false)
                throw new NotFoundException("Resume not found.");

            if (resume.JobSeekerProfileId != jobSeekerProfile.JobSeekerProfileId)
                throw new UnauthorizedException("You are not authorized to delete this resume.");

            resume.IsActive = false;

            await _resumeRepo.UpdateAsync(resume);
            await _resumeRepo.SaveChangesAsync();
        }
    }
}
