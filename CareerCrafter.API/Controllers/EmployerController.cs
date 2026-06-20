using Asp.Versioning;
using CareerCrafter.Core.DTOs;
using CareerCrafter.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CareerCrafter.API.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Employer")]
    public class EmployerController : ControllerBase
    {
        private readonly IEmployerService _service;
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(EmployerController));

        public EmployerController(IEmployerService service)
        {
            _service = service;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var result = await _service.GetProfileAsync(GetUserId());
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Warn($"GetProfile failed for user {GetUserId()} - {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateEmployerProfileDto dto)
        {
            try
            {
                var result = await _service.UpdateProfileAsync(GetUserId(), dto);
                _logger.Info($"User {GetUserId()} updated employer profile successfully");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Warn($"UpdateProfile failed for user {GetUserId()} - {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
