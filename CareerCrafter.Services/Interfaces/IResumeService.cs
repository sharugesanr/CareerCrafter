using CareerCrafter.Core.DTOs;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CareerCrafter.Services.Interfaces
{
    public interface IResumeService
    {
        Task<ResumeDto> UploadResumeAsync(int userId, IFormFile file);
        Task<List<ResumeDto>> GetMyResumesAsync(int userId);
        Task<ResumeDto> GetResumeByIdAsync(int userId, int resumeId);
        Task DeleteResumeAsync(int userId, int resumeId);
    }
}
