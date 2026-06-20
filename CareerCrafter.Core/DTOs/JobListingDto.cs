using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CareerCrafter.Core.DTOs
{
    public class JobListingDto
    {
        public int JobId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? JobType { get; set; }
        public string? SalaryRange { get; set; }
        public string? RequiredSkills { get; set; }
        public DateTime? PostedAt { get; set; }
        public DateTime? Deadline { get; set; }
        public bool? IsActive { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? Industry { get; set; }
    }

    public class CreateJobListingDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Location { get; set; }

        [MaxLength(50)]
        public string? JobType { get; set; }

        [MaxLength(100)]
        public string? SalaryRange { get; set; }

        public string? RequiredSkills { get; set; }

        public DateTime? Deadline { get; set; }
    }

    public class UpdateJobListingDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Location { get; set; }

        [MaxLength(50)]
        public string? JobType { get; set; }

        [MaxLength(100)]
        public string? SalaryRange { get; set; }

        public string? RequiredSkills { get; set; }

        public DateTime? Deadline { get; set; }
    }

    public class JobSearchDto
    {
        public string? Title { get; set; }
        public string? Location { get; set; }
        public string? JobType { get; set; }
        public string? CompanyName { get; set; }
        public string? SortBy { get; set; }        // "newest" or "oldest"
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class PagedResultDto<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}