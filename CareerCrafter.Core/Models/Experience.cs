using System;
using System.Collections.Generic;

namespace CareerCrafter.Core.Models
{

    public class Experience
    {
        public int ExperienceId { get; set; }

        public int JobSeekerProfileId { get; set; }

        public string JobTitle { get; set; } = null!;

        public string Company { get; set; } = null!;

        public string? Duration { get; set; }

        public string? Description { get; set; }

        public virtual JobSeekerProfile JobSeekerProfile { get; set; } = null!;
    }
}