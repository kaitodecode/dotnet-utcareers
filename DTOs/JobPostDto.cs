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
        public List<JobCategoryDto>? JobCategories { get; set; }
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
    }

    public class UpdateJobPostDto
    {
        [Required(ErrorMessage = "Company ID is required")]
        public Guid CompanyId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Thumbnail is required")]
        public IFormFile? Thumbnail { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [RegularExpression("^(active|closed)$", ErrorMessage = "Status must be either 'active' or 'closed'")]
        public string Status { get; set; } = null!;

        // List of job category IDs
        public List<Guid>? JobCategoryIds { get; set; }
    }
}