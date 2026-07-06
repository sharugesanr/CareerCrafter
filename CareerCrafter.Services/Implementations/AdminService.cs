using CareerCrafter.Core.DTOs;
using CareerCrafter.Core.Models;
using CareerCrafter.Repositories.Interfaces;
using CareerCrafter.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CareerCrafter.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly IAdminRepository _adminRepository;
        private readonly IWebHostEnvironment _environment;

        public AdminService(IAdminRepository adminRepository, IWebHostEnvironment environment)
        {
            _adminRepository = adminRepository;
            _environment = environment;
        }

        public async Task<AdminPurgeResultDto> PurgeDeletedResumesAsync()
        {
            var deletedResumes = await _adminRepository.GetSoftDeletedResumesAsync();

            if (!deletedResumes.Any())
            {
                return new AdminPurgeResultDto
                {
                    Deleted = 0,
                    Skipped = 0,
                    Message = "No soft-deleted resumes found to purge."
                };
            }

            var removableResumes = new List<Resume>();
            int skipped = 0;

            foreach (var resume in deletedResumes)
            {
                if (resume.Applications.Any())
                {
                    skipped++;
                    continue;
                }

                var fullPath = Path.Combine(_environment.WebRootPath, resume.FilePath);

                if (File.Exists(fullPath))
                    File.Delete(fullPath);

                removableResumes.Add(resume);
            }

            if (removableResumes.Any())
            {
                await _adminRepository.DeleteResumesAsync(removableResumes);
            }

            string message;

            if (removableResumes.Count == 0)
            {
                message = $"{skipped} resume(s) could not be deleted because they are linked to job applications.";
            }
            else if (skipped == 0)
            {
                message = $"{removableResumes.Count} resume(s) permanently deleted successfully.";
            }
            else
            {
                message = $"{removableResumes.Count} resume(s) permanently deleted. {skipped} resume(s) were retained because they are linked to job applications.";
            }

            return new AdminPurgeResultDto
            {
                Deleted = removableResumes.Count,
                Skipped = skipped,
                Message = message
            };
        }

        public async Task<List<AdminUserDto>> GetAllUsersAsync()
        {
            var users = await _adminRepository.GetAllUsersAsync();

            return users.Select(u => new AdminUserDto
            {
                UserId = u.UserId,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role,
                CreatedAt = u.CreatedAt
            }).ToList();
        }

        public async Task<List<AdminJobDto>> GetAllJobsAsync()
        {
            var jobs = await _adminRepository.GetAllJobsAsync();

            return jobs.Select(j => new AdminJobDto
            {
                JobId = j.JobId,
                Title = j.Title,
                CompanyName = j.EmployerProfile!.CompanyName,
                IsActive = j.IsActive,
                PostedAt = j.PostedAt
            }).ToList();
        }

        public async Task<PlatformStatsDto> GetPlatformStatsAsync()
        {
            return new PlatformStatsDto
            {
                TotalJobSeekers = await _adminRepository.CountUsersByRoleAsync("JobSeeker"),
                TotalEmployers = await _adminRepository.CountUsersByRoleAsync("Employer"),
                TotalActiveJobs = await _adminRepository.CountActiveJobsAsync(),
                TotalApplications = await _adminRepository.CountTotalApplicationsAsync()
            };
        }
    }
}
