using CareerCrafter.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CareerCrafter.Services.Interfaces
{
    public interface IJobListingService
    {
        Task<JobListingDto> CreateJobAsync(int userId, CreateJobListingDto dto);
        Task<JobListingDto> UpdateJobAsync(int userId, int jobId, UpdateJobListingDto dto);
        Task SoftDeleteJobAsync(int userId, int jobId);
        Task<JobListingDto?> GetJobByIdAsync(int jobId);

        Task<List<JobListingDto>> GetRecommendedJobsAsync(int userId);
        Task<PagedResultDto<JobListingDto>> SearchJobsAsync(JobSearchDto searchDto);
        Task<List<JobListingDto>> GetMyListingsAsync(int userId);
        Task ReactivateJobAsync(int userId, int jobId);

    }
}
