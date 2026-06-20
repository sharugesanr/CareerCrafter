using CareerCrafter.Core.DTOs;
using CareerCrafter.Core.Models;
using CareerCrafter.Repositories.Interfaces;
using CareerCrafter.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CareerCrafter.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repository;

        public NotificationService(INotificationRepository repository)
        {
            _repository = repository;
        }

        public async Task CreateNotificationAsync(int userId, string message)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            await _repository.AddAsync(notification);
            await _repository.SaveChangesAsync();
        }

        public async Task<List<NotificationDto>> GetMyNotificationsAsync(int userId)
        {
            var notifications = await _repository.GetByUserIdAsync(userId);
            return notifications.Select(n => new NotificationDto
            {
                NotificationId = n.NotificationId,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            }).ToList();
        }

        public async Task MarkAsReadAsync(int userId, int notificationId)
        {
            var notification = await _repository.GetByIdAsync(notificationId);
            if (notification == null)
                throw new Exception("Notification not found.");

            if (notification.UserId != userId)
                throw new Exception("You are not authorized to access this notification.");

            notification.IsRead = true;

            await _repository.UpdateAsync(notification);
            await _repository.SaveChangesAsync();
        }
    }
}
