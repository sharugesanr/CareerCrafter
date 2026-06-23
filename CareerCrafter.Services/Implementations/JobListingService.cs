using CareerCrafter.Core.DTOs;
using CareerCrafter.Core.Exceptions;
using CareerCrafter.Core.Models;
using CareerCrafter.Repositories.Interfaces;
using CareerCrafter.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CareerCrafter.Services.Implementations
{
    public class JobListingService : IJobListingService
    {
        private readonly IJobListingRepository _jobRepo;
        private readonly IEmployerRepository _employerRepo;

        public JobListingService(IJobListingRepository jobRepo, IEmployerRepository employerRepo)
        {
            _jobRepo = jobRepo;
            _employerRepo = employerRepo;
        }

        private static JobListingDto MapToDto(JobListing job)
        {
            return new JobListingDto
            {
                JobId = job.JobId,
                Title = job.Title,
                Description = job.Description,
                Location = job.Location,
                JobType = job.JobType,
                SalaryRange = job.SalaryRange,
                RequiredSkills = job.RequiredSkills,
                PostedAt = job.PostedAt,
                Deadline = job.Deadline,
                IsActive = job.IsActive,
                CompanyName = job.EmployerProfile.CompanyName,
                Industry = job.EmployerProfile.Industry
            };
        }

        public async Task<JobListingDto> CreateJobAsync(int userId, CreateJobListingDto dto)
        {
            var employer = await _employerRepo.GetByUserIdAsync(userId);
            if (employer == null)
                throw new NotFoundException("Employer profile not found.");

            if (dto.Deadline.HasValue && dto.Deadline.Value.Date <= DateTime.Now.Date)
                throw new ValidationException("Deadline must be a future date.");

            var job = new JobListing
            {
                EmployerProfileId = employer.EmployerProfileId,
                Title = dto.Title,
                Description = dto.Description,
                Location = dto.Location,
                JobType = dto.JobType,
                SalaryRange = dto.SalaryRange,
                RequiredSkills = dto.RequiredSkills,
                PostedAt = DateTime.Now,
                Deadline = dto.Deadline,
                IsActive = true
            };

            await _jobRepo.AddAsync(job);
            await _jobRepo.SaveChangesAsync();

            var saved = await _jobRepo.GetByIdAsync(job.JobId);
            return MapToDto(saved!);
        }

        public async Task<JobListingDto> UpdateJobAsync(int userId, int jobId, UpdateJobListingDto dto)
        {
            var employer = await _employerRepo.GetByUserIdAsync(userId);
            if (employer == null)
                throw new NotFoundException("Employer profile not found.");

            var job = await _jobRepo.GetByIdAsync(jobId);
            if (job == null)
                throw new NotFoundException("Job listing not found.");

            if (job.EmployerProfileId != employer.EmployerProfileId)
                throw new UnauthorizedException("You are not authorized to update this job.");

            if (dto.Deadline.HasValue && dto.Deadline.Value.Date <= DateTime.Now.Date)
                throw new ValidationException("Deadline must be a future date.");

            job.Title = dto.Title;
            job.Description = dto.Description;
            job.Location = dto.Location;
            job.JobType = dto.JobType;
            job.SalaryRange = dto.SalaryRange;
            job.RequiredSkills = dto.RequiredSkills;
            job.Deadline = dto.Deadline;

            await _jobRepo.UpdateAsync(job);
            await _jobRepo.SaveChangesAsync();

            return MapToDto(job);
        }

        public async Task SoftDeleteJobAsync(int userId, int jobId)
        {
            var employer = await _employerRepo.GetByUserIdAsync(userId);
            if (employer == null)
                throw new NotFoundException("Employer profile not found.");

            var job = await _jobRepo.GetByIdAsync(jobId);
            if (job == null)
                throw new NotFoundException("Job listing not found.");

            if (job.EmployerProfileId != employer.EmployerProfileId)
                throw new UnauthorizedException("You are not authorized to delete this job.");

            job.IsActive = false;

            await _jobRepo.UpdateAsync(job);
            await _jobRepo.SaveChangesAsync();
        }

        public async Task<JobListingDto?> GetJobByIdAsync(int jobId)
        {
            var job = await _jobRepo.GetByIdAsync(jobId);
            if (job == null || job.IsActive == false)
                return null;
            return MapToDto(job);
        }

        public async Task<PagedResultDto<JobListingDto>> SearchJobsAsync(JobSearchDto searchDto)
        {
            var jobs = await _jobRepo.GetAllActiveAsync();

            // Filtering
            if (!string.IsNullOrWhiteSpace(searchDto.Title))
                jobs = jobs.Where(j => j.Title.Contains(searchDto.Title, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(searchDto.Location))
                jobs = jobs.Where(j => j.Location != null && j.Location.Contains(searchDto.Location, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(searchDto.JobType))
                jobs = jobs.Where(j => j.JobType != null && j.JobType.Equals(searchDto.JobType, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(searchDto.CompanyName))
                jobs = jobs.Where(j => j.EmployerProfile.CompanyName.Contains(searchDto.CompanyName, StringComparison.OrdinalIgnoreCase)).ToList();

            // Sorting
            jobs = searchDto.SortBy?.ToLower() == "oldest"
                ? jobs.OrderBy(j => j.PostedAt).ToList()
                : jobs.OrderByDescending(j => j.PostedAt).ToList();

            // Pagination
            var totalCount = jobs.Count;
            var totalPages = (int)Math.Ceiling((double)totalCount / searchDto.PageSize);
            var pagedItems = jobs
                .Skip((searchDto.Page - 1) * searchDto.PageSize)
                .Take(searchDto.PageSize)
                .Select(j => MapToDto(j))
                .ToList();

            return new PagedResultDto<JobListingDto>
            {
                Items = pagedItems,
                TotalCount = totalCount,
                Page = searchDto.Page,
                PageSize = searchDto.PageSize,
                TotalPages = totalPages
            };
        }

        public async Task<List<JobListingDto>> GetMyListingsAsync(int userId)
        {
            var employer = await _employerRepo.GetByUserIdAsync(userId);
            if (employer == null)
                throw new NotFoundException("Employer profile not found.");

            var jobs = await _jobRepo.GetByEmployerProfileIdAsync(employer.EmployerProfileId);
            return jobs.Select(j => MapToDto(j)).ToList();
        }

        public async Task ReactivateJobAsync(int userId, int jobId)
        {
            var employer = await _employerRepo.GetByUserIdAsync(userId);
            if (employer == null)
                throw new NotFoundException("Employer profile not found.");

            var job = await _jobRepo.GetByIdAsync(jobId);
            if (job == null)
                throw new NotFoundException("Job listing not found.");

            if (job.EmployerProfileId != employer.EmployerProfileId)
                throw new UnauthorizedException("You are not authorized to reactivate this job.");

            job.IsActive = true;

            await _jobRepo.UpdateAsync(job);
            await _jobRepo.SaveChangesAsync();
        }
    }
}
