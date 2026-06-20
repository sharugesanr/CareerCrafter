using CareerCrafter.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CareerCrafter.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task AddAsync(Notification notification);
        Task<List<Notification>> GetByUserIdAsync(int userId);
        Task<Notification?> GetByIdAsync(int notificationId);
        Task UpdateAsync(Notification notification);
        Task SaveChangesAsync();
    }
}
