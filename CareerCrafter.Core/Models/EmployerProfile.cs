using System;
using System.Collections.Generic;

namespace CareerCrafter.Data;

public partial class EmployerProfile
{
    public int EmployerProfileId { get; set; }

    public int UserId { get; set; }

    public string CompanyName { get; set; } = null!;

    public string? Industry { get; set; }

    public string? Website { get; set; }

    public string? Location { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<JobListing> JobListings { get; set; } = new List<JobListing>();

    public virtual User User { get; set; } = null!;
}
