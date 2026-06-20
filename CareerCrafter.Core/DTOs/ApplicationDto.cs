using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CareerCrafter.Core.DTOs
{
    public class ApplicationDto
    {
        public int ApplicationId { get; set; }
        public int JobId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? Status { get; set; }
        public DateTime? AppliedAt { get; set; }
        public string? CoverNote { get; set; }
        public int ResumeId { get; set; }
    }

    public class CreateApplicationDto
    {
        [Required]
        public int JobId { get; set; }

        [Required]
        public int ResumeId { get; set; }

        [MaxLength(1000)]
        public string? CoverNote { get; set; }
    }

    public class UpdateApplicationStatusDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }

    public class ApplicantDto
    {
        public int ApplicationId { get; set; }
        public string JobSeekerName { get; set; } = string.Empty;
        public string JobSeekerEmail { get; set; } = string.Empty;
        public string? Status { get; set; }
        public DateTime? AppliedAt { get; set; }
        public string? CoverNote { get; set; }
        public int ResumeId { get; set; }
    }
}
