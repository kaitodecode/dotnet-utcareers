using System;
using System.Collections.Generic;

namespace dotnet_utcareers.Models;

public partial class JobPost
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Requirements { get; set; } = null!;

    public string Benefits { get; set; } = null!;

    public string Type { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<Applicant> Applicants { get; set; } = new List<Applicant>();

    public virtual Company Company { get; set; } = null!;

    public virtual ICollection<Selection> Selections { get; set; } = new List<Selection>();

    public virtual ICollection<JobCategory> JobCategories { get; set; } = new List<JobCategory>();
}
