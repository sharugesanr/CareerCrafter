using Asp.Versioning;
using CareerCrafter.Core.DTOs;
using CareerCrafter.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CareerCrafter.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize(Roles = "JobSeeker")]
    public class JobSeekerController : ControllerBase
    {
        private readonly IJobSeekerService _service;
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(JobSeekerController));

        public JobSeekerController(IJobSeekerService service)
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
                _logger.Info($"User {GetUserId()} fetched profile successfully");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Warn($"GetProfile failed for user {GetUserId()} - {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateJobSeekerProfileDto dto)
        {
            try
            {
                var result = await _service.UpdateProfileAsync(GetUserId(), dto);
                _logger.Info($"User {GetUserId()} updated profile successfully");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Warn($"UpdateProfile failed for user {GetUserId()} - {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("education")]
        public async Task<IActionResult> GetEducations()
        {
            try
            {
                var result = await _service.GetEducationsAsync(GetUserId());
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Warn($"GetEducations failed for user {GetUserId()} - {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("education")]
        public async Task<IActionResult> AddEducation([FromBody] AddEducationDto dto)
        {
            try
            {
                var result = await _service.AddEducationAsync(GetUserId(), dto);
                _logger.Info($"User {GetUserId()} added education record");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Warn($"AddEducation failed for user {GetUserId()} - {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("education/{educationId}")]
        public async Task<IActionResult> DeleteEducation(int educationId)
        {
            try
            {
                await _service.DeleteEducationAsync(GetUserId(), educationId);
                _logger.Info($"User {GetUserId()} deleted education {educationId}");
                return Ok(new { message = "Education deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.Warn($"DeleteEducation failed for user {GetUserId()} - {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("experience")]
        public async Task<IActionResult> GetExperiences()
        {
            try
            {
                var result = await _service.GetExperiencesAsync(GetUserId());
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Warn($"GetExperiences failed for user {GetUserId()} - {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("experience")]
        public async Task<IActionResult> AddExperience([FromBody] AddExperienceDto dto)
        {
            try
            {
                var result = await _service.AddExperienceAsync(GetUserId(), dto);
                _logger.Info($"User {GetUserId()} added experience record");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Warn($"AddExperience failed for user {GetUserId()} - {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("experience/{experienceId}")]
        public async Task<IActionResult> DeleteExperience(int experienceId)
        {
            try
            {
                await _service.DeleteExperienceAsync(GetUserId(), experienceId);
                _logger.Info($"User {GetUserId()} deleted experience {experienceId}");
                return Ok(new { message = "Experience deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.Warn($"DeleteExperience failed for user {GetUserId()} - {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
