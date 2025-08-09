using System;
using System.Collections.Generic;

namespace dotnet_utcareers.Models;

public partial class JobCategory
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<JobPostCategory> JobPostCategories { get; set; } = new List<JobPostCategory>();
}
