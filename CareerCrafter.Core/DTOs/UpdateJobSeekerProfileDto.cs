using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CareerCrafter.Core.DTOs
{
    public class UpdateJobSeekerProfileDto
    {
        public string? PhoneNumber { get; set; }
        public string? Location { get; set; }
        public string? Summary { get; set; }
        public string? Skills { get; set; }
    }
}
