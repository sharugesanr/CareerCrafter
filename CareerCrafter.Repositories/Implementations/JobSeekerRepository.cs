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
    public class JobSeekerRepository : IJobSeekerRepository
    {
        private readonly AppDbContext _context;

        public JobSeekerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<JobSeekerProfile?> GetProfileByUserIdAsync(int userId)
        {
            return await _context.JobSeekerProfiles
                .Include(p => p.User)
                .Include(p => p.Educations)
                .Include(p => p.Experiences)
                .Include(p => p.Resumes)
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }

        public async Task UpdateProfileAsync(JobSeekerProfile profile)
        {
            _context.JobSeekerProfiles.Update(profile);
        }

        public async Task<List<Education>> GetEducationsByProfileIdAsync(int profileId)
        {
            return await _context.Educations
                .Where(e => e.JobSeekerProfileId == profileId)
                .ToListAsync();
        }

        public async Task<Education?> GetEducationByIdAsync(int educationId)
        {
            return await _context.Educations.FindAsync(educationId);
        }

        public async Task AddEducationAsync(Education education)
        {
            await _context.Educations.AddAsync(education);
        }

        public async Task DeleteEducationAsync(Education education)
        {
            _context.Educations.Remove(education);
        }

        public async Task<List<Experience>> GetExperiencesByProfileIdAsync(int profileId)
        {
            return await _context.Experiences
                .Where(e => e.JobSeekerProfileId == profileId)
                .ToListAsync();
        }

        public async Task<Experience?> GetExperienceByIdAsync(int experienceId)
        {
            return await _context.Experiences.FindAsync(experienceId);
        }

        public async Task AddExperienceAsync(Experience experience)
        {
            await _context.Experiences.AddAsync(experience);
        }

        public async Task DeleteExperienceAsync(Experience experience)
        {
            _context.Experiences.Remove(experience);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
