using System;
using System.Collections.Generic;

namespace CareerCrafter.Data;

public partial class Application
{
    public int ApplicationId { get; set; }

    public int JobId { get; set; }

    public int JobSeekerProfileId { get; set; }

    public int ResumeId { get; set; }

    public string? Status { get; set; }

    public DateTime? AppliedAt { get; set; }

    public string? CoverNote { get; set; }

    public virtual JobListing Job { get; set; } = null!;

    public virtual JobSeekerProfile JobSeekerProfile { get; set; } = null!;

    public virtual Resume Resume { get; set; } = null!;
}
