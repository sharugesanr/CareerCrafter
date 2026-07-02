using CareerCrafter.Core.DTOs;
using CareerCrafter.Repositories.Interfaces;
using CareerCrafter.Services.Interfaces;
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

        public AdminService(IAdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
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
