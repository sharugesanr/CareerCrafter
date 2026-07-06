using CareerCrafter.Core.DTOs;
using CareerCrafter.Core.Models;
using CareerCrafter.Data;
using CareerCrafter.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CareerCrafter.Repositories.Implementations
{
    public class AdminRepository : IAdminRepository
    {
        private readonly AppDbContext _context;

        public AdminRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<JobListing>> GetAllJobsAsync()
        {
            return await _context.JobListings
                .Include(j => j.EmployerProfile)
                .OrderByDescending(j => j.PostedAt)
                .ToListAsync();
        }

        public async Task<int> CountUsersByRoleAsync(string role)
        {
            return await _context.Users
                .CountAsync(u => u.Role == role);
        }

        public async Task<int> CountActiveJobsAsync()
        {
            return await _context.JobListings
                .CountAsync(j => j.IsActive == true);
        }

        public async Task<int> CountTotalApplicationsAsync()
        {
            return await _context.Applications
                .CountAsync();
        }

        public async Task<List<Resume>> GetSoftDeletedResumesAsync()
        {
            return await _context.Resumes
                .Include(r=>r.Applications)
                .Where(r => r.IsActive == false)
                .ToListAsync();
        }

        public async Task DeleteResumesAsync(List<Resume> resumes)
        {
            _context.Resumes.RemoveRange(resumes);
            await _context.SaveChangesAsync();
        }



    }
}
