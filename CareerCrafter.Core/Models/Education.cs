using System;
using System.Collections.Generic;

namespace CareerCrafter.Core.Models
{

    public class Education
    {
        public int EducationId { get; set; }

        public int JobSeekerProfileId { get; set; }

        public string Degree { get; set; } = null!;

        public string Institution { get; set; } = null!;

        public int? YearOfPassing { get; set; }

        public virtual JobSeekerProfile JobSeekerProfile { get; set; } = null!;
    }
}