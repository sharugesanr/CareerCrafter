using AutoMapper;
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
    public class EmployerService : IEmployerService
    {
        private readonly IEmployerRepository _repository;

        public EmployerService(IEmployerRepository repository)
        {
            _repository = repository;
        }

        public async Task<EmployerProfileDto> GetProfileAsync(int userId)
        {
            var profile = await _repository.GetByUserIdAsync(userId);
            if (profile == null)
                throw new NotFoundException("Profile not found.");

            return new EmployerProfileDto
            {
                EmployerProfileId = profile.EmployerProfileId,
                CompanyName = profile.CompanyName,
                Industry = profile.Industry,
                Website = profile.Website,
                Location = profile.Location,
                Description = profile.Description,
                FullName = profile.User.FullName,
                Email = profile.User.Email
            };
        }

        public async Task<EmployerProfileDto> UpdateProfileAsync(int userId, UpdateEmployerProfileDto dto)
        {
            var profile = await _repository.GetByUserIdAsync(userId);
            if (profile == null)
                throw new NotFoundException("Profile not found.");

            profile.CompanyName = dto.CompanyName;
            profile.Industry = dto.Industry;
            profile.Website = dto.Website;
            profile.Location = dto.Location;
            profile.Description = dto.Description;

            await _repository.UpdateProfileAsync(profile);
            await _repository.SaveChangesAsync();

            return new EmployerProfileDto
            {
                EmployerProfileId = profile.EmployerProfileId,
                CompanyName = profile.CompanyName,
                Industry = profile.Industry,
                Website = profile.Website,
                Location = profile.Location,
                Description = profile.Description,
                FullName = profile.User.FullName,
                Email = profile.User.Email
            };
        }
    }
}
