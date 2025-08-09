using System;
using System.Collections.Generic;

namespace dotnet_utcareers.Models;

public partial class JobPost
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public string Title { get; set; } = null!;

    public string Thumbnail { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Company Company { get; set; } = null!;

    public virtual ICollection<JobPostCategory> JobPostCategories { get; set; } = new List<JobPostCategory>();
}
