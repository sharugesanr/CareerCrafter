using CareerCrafter.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CareerCrafter.Repositories.Interfaces
{
    public interface IResumeRepository
    {
        Task<Resume?> GetByIdAsync(int resumeId);
        Task<List<Resume>> GetActiveByJobSeekerProfileIdAsync(int jobSeekerProfileId);
        Task AddAsync(Resume resume);
        Task UpdateAsync(Resume resume);
        Task SaveChangesAsync();
    }
}
