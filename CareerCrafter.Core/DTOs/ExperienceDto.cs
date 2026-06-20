using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CareerCrafter.Core.DTOs
{
    public class ExperienceDto
    {
        public int ExperienceId { get; set; }
        public string JobTitle { get; set; } = null!;
        public string Company { get; set; } = null!;
        public string? Duration { get; set; }
        public string? Description { get; set; }
    }
}
