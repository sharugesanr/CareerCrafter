using System;
using System.Collections.Generic;

namespace CareerCrafter.Data;

public partial class JobSeekerProfile
{
    public int JobSeekerProfileId { get; set; }

    public int UserId { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Location { get; set; }

    public string? Summary { get; set; }

    public string? Skills { get; set; }

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    public virtual ICollection<Education> Educations { get; set; } = new List<Education>();

    public virtual ICollection<Experience> Experiences { get; set; } = new List<Experience>();

    public virtual ICollection<Resume> Resumes { get; set; } = new List<Resume>();

    public virtual User User { get; set; } = null!;
}
