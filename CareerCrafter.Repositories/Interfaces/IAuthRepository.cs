using CareerCrafter.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CareerCrafter.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<User> AddUserAsync(User user);
        Task AddEmployerProfileAsync(EmployerProfile profile);
        Task AddJobSeekerProfileAsync(JobSeekerProfile profile);

        Task AddPasswordResetOtpAsync(PasswordResetOtp otp);
        Task<PasswordResetOtp?> GetLatestOtpAsync(int userId, string otpCode);
        Task UpdateUserAsync(User user);
        Task InvalidatePreviousOtpsAsync(int userId);

        Task SaveChangesAsync();
    }
}
