using CareerCrafter.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CareerCrafter.Services.Interfaces
{
    public interface IAdminService
    {
        Task<List<AdminUserDto>> GetAllUsersAsync();

        Task<List<AdminJobDto>> GetAllJobsAsync();

        Task<PlatformStatsDto> GetPlatformStatsAsync();
    }
}
