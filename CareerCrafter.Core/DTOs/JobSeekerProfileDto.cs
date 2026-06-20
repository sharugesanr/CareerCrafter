using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CareerCrafter.Core.DTOs
{
    public class JobSeekerProfileDto
    {
        public int JobSeekerProfileId { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Location { get; set; }
        public string? Summary { get; set; }
        public string? Skills { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
    }
}
