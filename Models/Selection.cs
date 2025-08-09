using System;
using System.Collections.Generic;

namespace dotnet_utcareers.Models;

public partial class Selection
{
    public Guid Id { get; set; }

    public Guid ApplicantId { get; set; }

    public Guid JobPostCategoryId { get; set; }

    public string Stage { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? Attachment { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Applicant Applicant { get; set; } = null!;

    public virtual JobPostCategory JobPostCategory { get; set; } = null!;
}
