using CareerCrafter.Core.DTOs;
using CareerCrafter.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CareerCrafter.Repositories.Interfaces
{
    public interface IJobListingRepository
    {
        Task<JobListing?> GetByIdAsync(int jobId);
        Task<List<JobListing>> GetAllActiveAsync();
        Task<List<JobListing>> GetByEmployerProfileIdAsync(int employerProfileId);
        Task AddAsync(JobListing job);
        Task UpdateAsync(JobListing job);
        Task SaveChangesAsync();
    }

}