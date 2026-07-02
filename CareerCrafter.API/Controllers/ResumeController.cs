using Asp.Versioning;
using CareerCrafter.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CareerCrafter.API.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/resume")]
    [ApiController]
    
    public class ResumeController : ControllerBase
    {
        private readonly IResumeService _service;
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(ResumeController));

        public ResumeController(IResumeService service)
        {
            _service = service;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost("upload")]
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> UploadResume(IFormFile file)
        {
            try
            {
                var result = await _service.UploadResumeAsync(GetUserId(), file);
                _logger.Info($"User {GetUserId()} uploaded resume '{result.FileName}'");
                return StatusCode(201, result);
            }
            catch (Exception ex)
            {
                _logger.Warn($"UploadResume failed for user {GetUserId()} - {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> GetMyResumes()
        {
            try
            {
                var result = await _service.GetMyResumesAsync(GetUserId());
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Warn($"GetMyResumes failed for user {GetUserId()} - {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> GetResumeById(int id)
        {
            try
            {
                var result = await _service.GetResumeByIdAsync(GetUserId(), id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Warn($"GetResumeById failed for user {GetUserId()} on resume {id} - {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> DeleteResume(int id)
        {
            try
            {
                await _service.DeleteResumeAsync(GetUserId(), id);
                _logger.Info($"User {GetUserId()} deleted resume {id}");
                return Ok(new { message = "Resume deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.Warn($"DeleteResume failed for user {GetUserId()} on resume {id} - {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}/download")]
        [Authorize]
        public async Task<IActionResult> DownloadResume(int id)
        {
            try
            {
                var role = User.FindFirstValue(ClaimTypes.Role);
                var (bytes, fileName) = role == "Employer"
                    ? await _service.DownloadResumeForEmployerAsync(GetUserId(), id)
                    : await _service.DownloadResumeForJobSeekerAsync(GetUserId(), id);

                return File(bytes, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                _logger.Warn($"DownloadResume failed for user {GetUserId()} on resume {id} - {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
