using System;
using System.Collections.Generic;

namespace CareerCrafter.Data;

public partial class User
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Role { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual EmployerProfile? EmployerProfile { get; set; }

    public virtual JobSeekerProfile? JobSeekerProfile { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
