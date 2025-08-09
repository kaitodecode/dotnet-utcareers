using System;
using System.Collections.Generic;

namespace dotnet_utcareers.Models;

public partial class JobPostCategory
{
    public Guid Id { get; set; }

    public Guid JobCategoryId { get; set; }

    public Guid JobPostId { get; set; }

    public string Type { get; set; } = null!;

    public int RequiredCount { get; set; }

    public string? Description { get; set; }

    public string? Requirements { get; set; }

    public string? Benefits { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<Applicant> Applicants { get; set; } = new List<Applicant>();

    public virtual JobCategory JobCategory { get; set; } = null!;

    public virtual JobPost JobPost { get; set; } = null!;

    public virtual ICollection<Selection> Selections { get; set; } = new List<Selection>();
}
