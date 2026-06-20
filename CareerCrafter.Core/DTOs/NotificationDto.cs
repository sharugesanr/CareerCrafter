using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CareerCrafter.Core.DTOs
{
    public class NotificationDto
    {
        public int NotificationId { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool? IsRead { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
