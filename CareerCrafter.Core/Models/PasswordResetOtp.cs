using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CareerCrafter.Core.Models
{
    public class PasswordResetOtp
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string OtpCode { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
