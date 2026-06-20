using CareerCrafter.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CareerCrafter.Services.Interfaces
{
    public interface IJobSeekerService
    {
        Task<JobSeekerProfileDto> GetProfileAsync(int userId);
        Task<JobSeekerProfileDto> UpdateProfileAsync(int userId, UpdateJobSeekerProfileDto dto);

        Task<List<EducationDto>> GetEducationsAsync(int userId);
        Task<EducationDto> AddEducationAsync(int userId, AddEducationDto dto);
        Task DeleteEducationAsync(int userId, int educationId);

        Task<List<ExperienceDto>> GetExperiencesAsync(int userId);
        Task<ExperienceDto> AddExperienceAsync(int userId, AddExperienceDto dto);
        Task DeleteExperienceAsync(int userId, int experienceId);
    }
}
