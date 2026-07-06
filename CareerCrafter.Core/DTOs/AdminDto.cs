using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CareerCrafter.Core.DTOs
{
    public class AdminUserDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
    }

    public class AdminJobDto
    {
        public int JobId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public bool? IsActive { get; set; }
        public DateTime? PostedAt { get; set; }
    }

    public class PlatformStatsDto
    {
        public int TotalJobSeekers { get; set; }
        public int TotalEmployers { get; set; }
        public int TotalActiveJobs { get; set; }
        public int TotalApplications { get; set; }
    }

    public class AdminPurgeResultDto
    {
        public int Deleted { get; set; }

        public int Skipped { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}