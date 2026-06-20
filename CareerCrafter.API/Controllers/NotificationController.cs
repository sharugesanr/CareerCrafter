using Asp.Versioning;
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
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _service;
        private static readonly log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(NotificationController));

        public NotificationController(INotificationService service)
        {
            _service = service;
        }

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> GetMyNotifications()
        {
            try
            {
                var result = await _service.GetMyNotificationsAsync(GetUserId());
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.Warn($"GetMyNotifications failed for user {GetUserId()} - {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPatch("{id}/mark-read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                await _service.MarkAsReadAsync(GetUserId(), id);
                return Ok(new { message = "Notification marked as read." });
            }
            catch (Exception ex)
            {
                _logger.Warn($"MarkAsRead failed for user {GetUserId()} on notification {id} - {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
