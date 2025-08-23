using System.ComponentModel.DataAnnotations;
using dotnet_utcareers.Models;

namespace dotnet_utcareers.DTOs
{
    public class JobPostDto
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public string Title { get; set; } = null!;
        public string Thumbnail { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties as DTOs
        public CompanyDto? Company { get; set; }
        public List<JobPostCategoryDetailDto>? JobCategories { get; set; } = new List<JobPostCategoryDetailDto>();
    }

    public class JobPostCategoryDetailDto
    {
        public Guid Id { get; set; }
        public JobCategoryDto? JobCategory { get; set; }
        public string Type { get; set; } = null!;
        public int RequiredCount { get; set; }
        public string? Description { get; set; }
        public string? Requirements { get; set; }
        public string? Benefits { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateJobPostDto
    {
        [Required(ErrorMessage = "Company ID is required")]
        public Guid CompanyId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Thumbnail is required")]
        public IFormFile Thumbnail { get; set; } = null!;

        [Required(ErrorMessage = "Status is required")]
        [RegularExpression("^(active|closed)$", ErrorMessage = "Status must be either 'active' or 'closed'")]
        public string Status { get; set; } = "active";

        public List<JobPostCategoryDto>? JobCategories { get; set; }
    }

    public class UpdateJobPostDto
    {
        [Required(ErrorMessage = "Company ID is required")]
        public Guid CompanyId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = null!;

        public IFormFile? Thumbnail { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [RegularExpression("^(active|closed)$", ErrorMessage = "Status must be either 'active' or 'closed'")]
        public string Status { get; set; } = null!;

        // List of job category IDs
        public List<JobPostCategoryDto>? JobCategories { get; set; }
    }

    public class JobPostCategoryDto
    {
        [Required(ErrorMessage = "Job Category ID is required")]
        public Guid JobCategoryId { get; set; }

        [Required(ErrorMessage = "Type is required")]
        [StringLength(100, ErrorMessage = "Type cannot exceed 100 characters")]
        public string Type { get; set; } = null!;

        [Required(ErrorMessage = "Required Count is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Required Count must be greater than 0")]
        public int RequiredCount { get; set; }

        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string? Description { get; set; }

        [StringLength(2000, ErrorMessage = "Requirements cannot exceed 2000 characters")]
        public string? Requirements { get; set; }

        [StringLength(2000, ErrorMessage = "Benefits cannot exceed 2000 characters")]
        public string? Benefits { get; set; }
    }
}