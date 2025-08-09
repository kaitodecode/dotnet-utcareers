using System;
using System.Collections.Generic;

namespace dotnet_utcareers.Models;

public partial class Applicant
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid JobId { get; set; }

    public string Status { get; set; } = null!;

    public string Cv { get; set; } = null!;

    public string NationalIdentityCard { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual JobPost Job { get; set; } = null!;

    public virtual ICollection<Selection> Selections { get; set; } = new List<Selection>();

    public virtual User User { get; set; } = null!;
}
