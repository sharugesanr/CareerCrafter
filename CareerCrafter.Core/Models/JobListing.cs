using System;
using System.Collections.Generic;

namespace CareerCrafter.Core.Models
{

    public class JobListing
    {
        public int JobId { get; set; }

        public int EmployerProfileId { get; set; }

        public string Title { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string? Location { get; set; }

        public string? JobType { get; set; }

        public string? SalaryRange { get; set; }

        public string? RequiredSkills { get; set; }

        public DateTime? PostedAt { get; set; }

        public DateTime? Deadline { get; set; }

        public bool? IsActive { get; set; }

        public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

        public virtual EmployerProfile EmployerProfile { get; set; } = null!;
    }
}