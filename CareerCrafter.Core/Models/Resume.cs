using System;
using System.Collections.Generic;

namespace CareerCrafter.Data;

public partial class Resume
{
    public int ResumeId { get; set; }

    public int JobSeekerProfileId { get; set; }

    public string FileName { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public DateTime? UploadedAt { get; set; }

    public bool? IsActive { get; set; }

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    public virtual JobSeekerProfile JobSeekerProfile { get; set; } = null!;
}
