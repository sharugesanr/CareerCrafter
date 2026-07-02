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
    public class ApplicationRepository : IApplicationRepository
    {
        private readonly AppDbContext _context;

        public ApplicationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Application?> GetByIdAsync(int applicationId)
        {
            return await _context.Applications
                    .Include(a => a.Job)
                    .ThenInclude(j => j.EmployerProfile)
                    .Include(a => a.JobSeekerProfile)
                    .ThenInclude(p => p.User)
                    .Include(a => a.Resume)
                    .FirstOrDefaultAsync(a => a.ApplicationId == applicationId);
        }

        public async Task<Application?> GetByJobAndJobSeekerAsync(int jobId, int jobSeekerProfileId)
        {
            return await _context.Applications
                .FirstOrDefaultAsync(a => a.JobId == jobId && a.JobSeekerProfileId == jobSeekerProfileId);
        }

        public async Task<List<Application>> GetByJobSeekerProfileIdAsync(int jobSeekerProfileId)
        {
            return await _context.Applications
                .Include(a => a.Job)
                    .ThenInclude(j => j.EmployerProfile)
                .Include(a => a.Resume)
                .Where(a => a.JobSeekerProfileId == jobSeekerProfileId)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();
        }

        public async Task<List<Application>> GetByJobIdAsync(int jobId)
        {
            return await _context.Applications
                .Include(a => a.JobSeekerProfile)
                    .ThenInclude(p => p.User)
                .Include(a => a.Resume)
                .Where(a => a.JobId == jobId)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();
        }

        public async Task AddAsync(Application application)
        {
            await _context.Applications.AddAsync(application);
        }

        public async Task UpdateAsync(Application application)
        {
            _context.Applications.Update(application);
        }

        public async Task<bool> ExistsForResumeAndEmployerAsync(int resumeId, int employerProfileId)
        {
            return await _context.Applications
                .Include(a => a.Job)
                .AnyAsync(a => a.ResumeId == resumeId && a.Job.EmployerProfileId == employerProfileId);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
