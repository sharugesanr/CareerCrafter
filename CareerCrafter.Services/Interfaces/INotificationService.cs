using CareerCrafter.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CareerCrafter.Services.Interfaces
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(int userId, string message);
        Task<List<NotificationDto>> GetMyNotificationsAsync(int userId);
        Task MarkAsReadAsync(int userId, int notificationId);
    }
}
