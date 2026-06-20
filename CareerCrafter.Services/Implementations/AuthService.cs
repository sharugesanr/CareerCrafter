using CareerCrafter.Core.DTOs;
using CareerCrafter.Core.Models;
using CareerCrafter.Data;
using CareerCrafter.Repositories.Interfaces;
using CareerCrafter.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CareerCrafter.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;

        public AuthService(
            IAuthRepository authRepository,
            IConfiguration configuration,
            IEmailService emailService,
            INotificationService notificationService)
        {
            _authRepository = authRepository;
            _configuration = configuration;
            _emailService = emailService;
            _notificationService = notificationService;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var existingUser = await _authRepository.GetUserByEmailAsync(dto.Email);
            if (existingUser != null)
                throw new Exception("Email already registered.");

            if (dto.Role != "JobSeeker" && dto.Role != "Employer")
                throw new Exception("Invalid role. Must be JobSeeker or Employer.");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = passwordHash,
                Role = dto.Role,
                CreatedAt = DateTime.Now
            };

            await _authRepository.AddUserAsync(user);

            if (dto.Role == "Employer")
            {
                await _authRepository.AddEmployerProfileAsync(new EmployerProfile
                {
                    UserId = user.UserId,
                    CompanyName = dto.CompanyName ?? "Not Specified"
                });
            }
            else
            {
                await _authRepository.AddJobSeekerProfileAsync(new JobSeekerProfile
                {
                    UserId = user.UserId
                });
            }

            await _authRepository.SaveChangesAsync();
            try
            {
                await _emailService.SendEmailAsync(
                    user.Email,
                    "Welcome to CareerCrafter!",
                    $"Hi {user.FullName},\n\nYour CareerCrafter account has been created successfully as a {user.Role}.\n\nStart exploring opportunities today!\n\n- CareerCrafter Team");

                await _notificationService.CreateNotificationAsync(user.UserId, "Welcome to CareerCrafter! Your account has been created successfully.");
            }
            catch (Exception)
            {
                // Email/notification failure should not break registration
            }
            var token = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                Token = token,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                UserId = user.UserId
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _authRepository.GetUserByEmailAsync(dto.Email);

            if (user == null)
                throw new Exception("Invalid email or password.");

            var isValidPassword = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);

            if (!isValidPassword)
                throw new Exception("Invalid email or password.");

            var token = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                Token = token,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                UserId = user.UserId
            };
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"]!;
            var issuer = jwtSettings["Issuer"]!;
            var audience = jwtSettings["Audience"]!;
            var expiryInMinutes = int.Parse(jwtSettings["ExpiryInMinutes"]!);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Name, user.FullName)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(expiryInMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}


   