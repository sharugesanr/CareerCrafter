using CareerCrafter.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CareerCrafter.Services.Interfaces
{
    public interface IEmployerService
    {
        Task<EmployerProfileDto> GetProfileAsync(int userId);
        Task<EmployerProfileDto> UpdateProfileAsync(int userId, UpdateEmployerProfileDto dto);
    }
}
