using CareerCrafter.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CareerCrafter.Repositories.Interfaces
{
    public interface IApplicationRepository
    {
        Task<Application?> GetByIdAsync(int applicationId);
        Task<Application?> GetByJobAndJobSeekerAsync(int jobId, int jobSeekerProfileId);
        Task<List<Application>> GetByJobSeekerProfileIdAsync(int jobSeekerProfileId);
        Task<List<Application>> GetByJobIdAsync(int jobId);
        Task AddAsync(Application application);
        Task UpdateAsync(Application application);
        Task<bool> ExistsForResumeAndEmployerAsync(int resumeId, int employerProfileId);
        Task SaveChangesAsync();
    }
}
