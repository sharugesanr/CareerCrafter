using CareerCrafter.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CareerCrafter.Services.Interfaces
{
    public interface IApplicationService
    {
        Task<ApplicationDto> ApplyAsync(int userId, CreateApplicationDto dto);
        Task<List<ApplicationDto>> GetMyApplicationsAsync(int userId);
        Task<ApplicationDto> GetApplicationByIdAsync(int userId, int applicationId);
        Task WithdrawApplicationAsync(int userId, int applicationId);
        Task<List<ApplicantDto>> GetApplicantsByJobAsync(int userId, int jobId);
        Task<ApplicantDto> UpdateApplicationStatusAsync(int userId, int applicationId, UpdateApplicationStatusDto dto);
    }
}
