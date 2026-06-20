using CareerCrafter.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CareerCrafter.Repositories.Interfaces
{
    public interface IJobSeekerRepository
    {
        Task<JobSeekerProfile?> GetProfileByUserIdAsync(int userId);
        Task UpdateProfileAsync(JobSeekerProfile profile);

        Task<List<Education>> GetEducationsByProfileIdAsync(int profileId);
        Task<Education?> GetEducationByIdAsync(int educationId);
        Task AddEducationAsync(Education education);
        Task DeleteEducationAsync(Education education);

        Task<List<Experience>> GetExperiencesByProfileIdAsync(int profileId);
        Task<Experience?> GetExperienceByIdAsync(int experienceId);
        Task AddExperienceAsync(Experience experience);
        Task DeleteExperienceAsync(Experience experience);

        Task SaveChangesAsync();
    }
}
