using CareerCrafter.Core.Models;
using CareerCrafter.Data;
using CareerCrafter.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CareerCrafter.Repositories.Implementations
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AppDbContext _context;

        public AuthRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> AddUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task AddEmployerProfileAsync(EmployerProfile profile)
        {
            _context.EmployerProfiles.Add(profile);
        }

        public async Task AddJobSeekerProfileAsync(JobSeekerProfile profile)
        {
            _context.JobSeekerProfiles.Add(profile);
        }

        public async Task AddPasswordResetOtpAsync(PasswordResetOtp otp)
        {
            await _context.PasswordResetOtps.AddAsync(otp);
        }

        public async Task<PasswordResetOtp?> GetLatestOtpAsync(int userId, string otpCode)
        {
            return await _context.PasswordResetOtps
                .Where(o =>
                    o.UserId == userId &&
                    o.OtpCode == otpCode &&
                    !o.IsUsed &&
                    o.ExpiresAt > DateTime.Now)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await Task.CompletedTask;
        }

        public async Task InvalidatePreviousOtpsAsync(int userId)
        {
            var otps = await _context.PasswordResetOtps
                .Where(o => o.UserId == userId && !o.IsUsed)
                .ToListAsync();

            foreach (var otp in otps)
            {
                otp.IsUsed = true;
            }
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
