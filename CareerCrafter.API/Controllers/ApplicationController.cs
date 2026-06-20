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
    [Route("api/v{version:apiVersion}/applications")]
    [ApiController]
    public class ApplicationController : ControllerBase
    {
        private readonly IApplicationService _service;
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(ApplicationController));

        public ApplicationController(IApplicationService service)
        {
            _service = service;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost]
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> Apply([FromBody] CreateApplicationDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _service.ApplyAsync(GetUserId(), dto);
                _logger.Info($"User {GetUserId()} applied to job {dto.JobId}");
                return StatusCode(201, result);
            }
            catch (Exception ex)
            {
                _logger.Warn($"Apply failed for user {GetUserId()} on job {dto.JobId} - {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("my-applications")]
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> GetMyApplications()
        {
            try
            {
                var result = await _service.GetMyApplicationsAsync(GetUserId());
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Warn($"GetMyApplications failed for user {GetUserId()} - {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> GetApplicationById(int id)
        {
            try
            {
                var result = await _service.GetApplicationByIdAsync(GetUserId(), id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Warn($"GetApplicationById failed for user {GetUserId()} on application {id} - {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("{id}/withdraw")]
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> WithdrawApplication(int id)
        {
            try
            {
                await _service.WithdrawApplicationAsync(GetUserId(), id);
                _logger.Info($"User {GetUserId()} withdrew application {id}");
                return Ok(new { message = "Application withdrawn successfully." });
            }
            catch (Exception ex)
            {
                _logger.Warn($"WithdrawApplication failed for user {GetUserId()} on application {id} - {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("job/{jobId}")]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> GetApplicantsByJob(int jobId)
        {
            try
            {
                var result = await _service.GetApplicantsByJobAsync(GetUserId(), jobId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Warn($"GetApplicantsByJob failed for user {GetUserId()} on job {jobId} - {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Employer")]
        public async Task<IActionResult> UpdateApplicationStatus(int id, [FromBody] UpdateApplicationStatusDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _service.UpdateApplicationStatusAsync(GetUserId(), id, dto);
                _logger.Info($"User {GetUserId()} updated application {id} status to {dto.Status}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Warn($"UpdateApplicationStatus failed for user {GetUserId()} on application {id} - {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
