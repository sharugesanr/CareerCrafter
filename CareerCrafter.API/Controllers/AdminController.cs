using Asp.Versioning;
using CareerCrafter.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareerCrafter.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(AdminController));

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetPlatformStats()
        {
            try
            {
                var result = await _adminService.GetPlatformStatsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Warn($"GetPlatformStats failed - {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var result = await _adminService.GetAllUsersAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Warn($"GetAllUsers failed - {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("jobs")]
        public async Task<IActionResult> GetAllJobs()
        {
            try
            {
                var result = await _adminService.GetAllJobsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Warn($"GetAllJobs failed - {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}