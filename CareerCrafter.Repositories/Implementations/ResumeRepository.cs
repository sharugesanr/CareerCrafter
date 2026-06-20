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
    public class ResumeRepository : IResumeRepository
    {
        private readonly AppDbContext _context;

        public ResumeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Resume?> GetByIdAsync(int resumeId)
        {
            return await _context.Resumes
                .Include(r => r.JobSeekerProfile)
                .FirstOrDefaultAsync(r => r.ResumeId == resumeId);
        }

        public async Task<List<Resume>> GetActiveByJobSeekerProfileIdAsync(int jobSeekerProfileId)
        {
            return await _context.Resumes
                .Where(r => r.JobSeekerProfileId == jobSeekerProfileId && r.IsActive == true)
                .OrderByDescending(r => r.UploadedAt)
                .ToListAsync();
        }

        public async Task AddAsync(Resume resume)
        {
            await _context.Resumes.AddAsync(resume);
        }

        public async Task UpdateAsync(Resume resume)
        {
            _context.Resumes.Update(resume);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
