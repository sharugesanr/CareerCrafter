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

    public class JobListingRepository : IJobListingRepository
    {
        private readonly AppDbContext _context;

        public JobListingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<JobListing?> GetByIdAsync(int jobId)
        {
            return await _context.JobListings
                .Include(j => j.EmployerProfile)
                .FirstOrDefaultAsync(j => j.JobId == jobId);
        }

        public async Task<List<JobListing>> GetAllActiveAsync()
        {
            return await _context.JobListings
                .Include(j => j.EmployerProfile)
                .Where(j => j.IsActive == true)
                .ToListAsync();
        }

        public async Task<List<JobListing>> GetByEmployerProfileIdAsync(int employerProfileId)
        {
            return await _context.JobListings
                .Include(j => j.EmployerProfile)
                .Where(j => j.EmployerProfileId == employerProfileId)
                .ToListAsync();
        }

        public async Task AddAsync(JobListing job)
        {
            await _context.JobListings.AddAsync(job);
        }

        public async Task UpdateAsync(JobListing job)
        {
            _context.JobListings.Update(job);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
