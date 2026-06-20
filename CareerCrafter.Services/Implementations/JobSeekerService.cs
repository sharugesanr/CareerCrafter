using CareerCrafter.Core.DTOs;
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
    public class JobSeekerService : IJobSeekerService
    {
        private readonly IJobSeekerRepository _repository;

        public JobSeekerService(IJobSeekerRepository repository)
        {
            _repository = repository;
        }

        public async Task<JobSeekerProfileDto> GetProfileAsync(int userId)
        {
            var profile = await _repository.GetProfileByUserIdAsync(userId);
            if (profile == null)
                throw new Exception("Profile not found.");

            return new JobSeekerProfileDto
            {
                JobSeekerProfileId = profile.JobSeekerProfileId,
                PhoneNumber = profile.PhoneNumber,
                Location = profile.Location,
                Summary = profile.Summary,
                Skills = profile.Skills,
                FullName = profile.User.FullName,
                Email = profile.User.Email
            };
        }

        public async Task<JobSeekerProfileDto> UpdateProfileAsync(int userId, UpdateJobSeekerProfileDto dto)
        {
            var profile = await _repository.GetProfileByUserIdAsync(userId);
            if (profile == null)
                throw new Exception("Profile not found.");

            profile.PhoneNumber = dto.PhoneNumber;
            profile.Location = dto.Location;
            profile.Summary = dto.Summary;
            profile.Skills = dto.Skills;

            await _repository.UpdateProfileAsync(profile);
            await _repository.SaveChangesAsync();

            return new JobSeekerProfileDto
            {
                JobSeekerProfileId = profile.JobSeekerProfileId,
                PhoneNumber = profile.PhoneNumber,
                Location = profile.Location,
                Summary = profile.Summary,
                Skills = profile.Skills,
                FullName = profile.User.FullName,
                Email = profile.User.Email
            };
        }

        public async Task<List<EducationDto>> GetEducationsAsync(int userId)
        {
            var profile = await _repository.GetProfileByUserIdAsync(userId);
            if (profile == null)
                throw new Exception("Profile not found.");

            var educations = await _repository.GetEducationsByProfileIdAsync(profile.JobSeekerProfileId);

            return educations.Select(e => new EducationDto
            {
                EducationId = e.EducationId,
                Degree = e.Degree,
                Institution = e.Institution,
                YearOfPassing = e.YearOfPassing
            }).ToList();
        }

        public async Task<EducationDto> AddEducationAsync(int userId, AddEducationDto dto)
        {
            var profile = await _repository.GetProfileByUserIdAsync(userId);
            if (profile == null)
                throw new Exception("Profile not found.");

            var education = new Education
            {
                JobSeekerProfileId = profile.JobSeekerProfileId,
                Degree = dto.Degree,
                Institution = dto.Institution,
                YearOfPassing = dto.YearOfPassing
            };

            await _repository.AddEducationAsync(education);
            await _repository.SaveChangesAsync();

            return new EducationDto
            {
                EducationId = education.EducationId,
                Degree = education.Degree,
                Institution = education.Institution,
                YearOfPassing = education.YearOfPassing
            };
        }

        public async Task DeleteEducationAsync(int userId, int educationId)
        {
            var profile = await _repository.GetProfileByUserIdAsync(userId);
            if (profile == null)
                throw new Exception("Profile not found.");

            var education = await _repository.GetEducationByIdAsync(educationId);
            if (education == null || education.JobSeekerProfileId != profile.JobSeekerProfileId)
                throw new Exception("Education record not found.");

            await _repository.DeleteEducationAsync(education);
            await _repository.SaveChangesAsync();
        }

        public async Task<List<ExperienceDto>> GetExperiencesAsync(int userId)
        {
            var profile = await _repository.GetProfileByUserIdAsync(userId);
            if (profile == null)
                throw new Exception("Profile not found.");

            var experiences = await _repository.GetExperiencesByProfileIdAsync(profile.JobSeekerProfileId);

            return experiences.Select(e => new ExperienceDto
            {
                ExperienceId = e.ExperienceId,
                JobTitle = e.JobTitle,
                Company = e.Company,
                Duration = e.Duration,
                Description = e.Description
            }).ToList();
        }

        public async Task<ExperienceDto> AddExperienceAsync(int userId, AddExperienceDto dto)
        {
            var profile = await _repository.GetProfileByUserIdAsync(userId);
            if (profile == null)
                throw new Exception("Profile not found.");

            var experience = new Experience
            {
                JobSeekerProfileId = profile.JobSeekerProfileId,
                JobTitle = dto.JobTitle,
                Company = dto.Company,
                Duration = dto.Duration,
                Description = dto.Description
            };

            await _repository.AddExperienceAsync(experience);
            await _repository.SaveChangesAsync();

            return new ExperienceDto
            {
                ExperienceId = experience.ExperienceId,
                JobTitle = experience.JobTitle,
                Company = experience.Company,
                Duration = experience.Duration,
                Description = experience.Description
            };
        }

        public async Task DeleteExperienceAsync(int userId, int experienceId)
        {
            var profile = await _repository.GetProfileByUserIdAsync(userId);
            if (profile == null)
                throw new Exception("Profile not found.");

            var experience = await _repository.GetExperienceByIdAsync(experienceId);
            if (experience == null || experience.JobSeekerProfileId != profile.JobSeekerProfileId)
                throw new Exception("Experience record not found.");

            await _repository.DeleteExperienceAsync(experience);
            await _repository.SaveChangesAsync();
        }
    }
}
