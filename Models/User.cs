using System;
using System.Collections.Generic;

namespace dotnet_utcareers.Models;

public partial class User
{
    public Guid Id { get; set; }

    public string? Photo { get; set; }

    public string Name { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Address { get; set; }

    public string? Description { get; set; }

    public string Password { get; set; } = null!;

    public string Role { get; set; } = null!;

    public DateTime? VerifiedAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<Applicant> Applicants { get; set; } = new List<Applicant>();
}
