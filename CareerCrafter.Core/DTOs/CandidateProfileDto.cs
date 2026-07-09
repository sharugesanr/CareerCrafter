using System;
using System.Collections.Generic;

namespace CareerCrafter.Core.DTOs
{
    public class CandidateProfileDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Location { get; set; }
        public string? Summary { get; set; }
        public string? Skills { get; set; }

        public List<CandidateEducationDto> Educations { get; set; } = new();
        public List<CandidateExperienceDto> Experiences { get; set; } = new();

        public int ResumeId { get; set; }
        public string ResumeFileName { get; set; } = string.Empty;
        public DateTime? ResumeUploadedAt { get; set; }
    }

    public class CandidateEducationDto
    {
        public string Degree { get; set; } = string.Empty;
        public string Institution { get; set; } = string.Empty;
        public int? YearOfPassing { get; set; }
    }

    public class CandidateExperienceDto
    {
        public string JobTitle { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string? Duration { get; set; }
        public string? Description { get; set; }
    }
}