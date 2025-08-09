using System.ComponentModel.DataAnnotations;

namespace dotnet_utcareers.DTOs
{
    public class JobPostDto
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
        
        // Navigation properties as DTOs
        public CompanyDto? Company { get; set; }
        public List<JobCategoryDto>? JobCategories { get; set; }
    }

    public class CreateJobPostDto
    {
        [Required(ErrorMessage = "Company ID is required")]
        public Guid CompanyId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "Requirements is required")]
        [StringLength(1000, ErrorMessage = "Requirements cannot exceed 1000 characters")]
        public string Requirements { get; set; } = null!;

        [Required(ErrorMessage = "Benefits is required")]
        [StringLength(1000, ErrorMessage = "Benefits cannot exceed 1000 characters")]
        public string Benefits { get; set; } = null!;

        [Required(ErrorMessage = "Type is required")]
        [StringLength(50, ErrorMessage = "Type cannot exceed 50 characters")]
        public string Type { get; set; } = null!;

        [Required(ErrorMessage = "Status is required")]
        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        public string Status { get; set; } = null!;

        // List of job category IDs
        public List<Guid>? JobCategoryIds { get; set; }
    }

    public class UpdateJobPostDto
    {
        [Required(ErrorMessage = "Company ID is required")]
        public Guid CompanyId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "Requirements is required")]
        [StringLength(1000, ErrorMessage = "Requirements cannot exceed 1000 characters")]
        public string Requirements { get; set; } = null!;

        [Required(ErrorMessage = "Benefits is required")]
        [StringLength(1000, ErrorMessage = "Benefits cannot exceed 1000 characters")]
        public string Benefits { get; set; } = null!;

        [Required(ErrorMessage = "Type is required")]
        [StringLength(50, ErrorMessage = "Type cannot exceed 50 characters")]
        public string Type { get; set; } = null!;

        [Required(ErrorMessage = "Status is required")]
        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        public string Status { get; set; } = null!;

        // List of job category IDs
        public List<Guid>? JobCategoryIds { get; set; }
    }
}