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
    [Route("api/v{version:apiVersion}/jobs")]
    [ApiController]
    public class JobListingController : ControllerBase
    {
        private readonly IJobListingService _service;
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(JobListingController));
        public JobListingController(IJobListingService service)
        {
            _service = service;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> SearchJobs([FromQuery] JobSearchDto searchDto)
        {
            try
            {
                var result = await _service.SearchJobsAsync(searchDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Warn($"SearchJobs failed - {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("recommended")]
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> GetRecommendedJobs()
        {
            try
            {
                var result = await _service.GetRecommendedJobsAsync(GetUserId());
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Warn($"GetRecommendedJobs failed for user {GetUserId()} - {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetJobById(int id)
        {
            try
            {
                var result = await _service.GetJobByIdAsync(id);
                if (result == null)
                    return NotFound(new { message = "Job not found." });
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Warn($"GetJobById failed for job {id} - {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("my-listings")]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> GetMyListings()
        {
            try
            {
                var result = await _service.GetMyListingsAsync(GetUserId());
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Warn($"GetMyListings failed for user {GetUserId()} - {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> CreateJob([FromBody] CreateJobListingDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _service.CreateJobAsync(GetUserId(), dto);
                _logger.Info($"User {GetUserId()} created job listing '{dto.Title}'");
                return StatusCode(201, result);
            }
            catch (Exception ex)
            {
                _logger.Warn($"CreateJob failed for user {GetUserId()} - {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> UpdateJob(int id, [FromBody] UpdateJobListingDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _service.UpdateJobAsync(GetUserId(), id, dto);
                _logger.Info($"User {GetUserId()} updated job listing {id}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Warn($"UpdateJob failed for user {GetUserId()} on job {id} - {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> SoftDeleteJob(int id)
        {
            try
            {
                await _service.SoftDeleteJobAsync(GetUserId(), id);
                _logger.Info($"User {GetUserId()} deactivated job listing {id}");
                return Ok(new { message = "Job listing deactivated successfully." });
            }
            catch (Exception ex)
            {
                _logger.Warn($"SoftDeleteJob failed for user {GetUserId()} on job {id} - {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("{id}/reactivate")]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> ReactivateJob(int id)
        {
            try
            {
                await _service.ReactivateJobAsync(GetUserId(), id);
                _logger.Info($"User {GetUserId()} reactivated job listing {id}");
                return Ok(new { message = "Job listing reactivated successfully." });
            }
            catch (Exception ex)
            {
                _logger.Warn($"ReactivateJob failed for user {GetUserId()} on job {id} - {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
