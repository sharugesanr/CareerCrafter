using Asp.Versioning;
using CareerCrafter.Core.DTOs;
using CareerCrafter.Services.Interfaces;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CareerCrafter.API.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AuthController));

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            try
            {
                var result = await _authService.RegisterAsync(dto);
                _logger.Info($"User {dto.Email} registered successfully as {dto.Role}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Warn($"Registration failed for {dto.Email} - {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var result = await _authService.LoginAsync(dto);
                _logger.Info($"User {dto.Email} logged in successfully");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Warn($"Failed login attempt for {dto.Email} - {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}