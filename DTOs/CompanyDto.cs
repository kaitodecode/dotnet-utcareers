using System.ComponentModel.DataAnnotations;

namespace dotnet_utcareers.DTOs
{
    public class CompanyDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? Website { get; set; }
        public string? Logo { get; set; }
        public string Location { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateCompanyDto
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Phone is required")]
        [Phone(ErrorMessage = "Invalid phone format")]
        public string Phone { get; set; } = null!;

        [Url(ErrorMessage = "Invalid website URL format")]
        [Required(ErrorMessage = "Website is required")]
        public string Website { get; set; } = null!;

        [Required(ErrorMessage = "Logo is required")]
        public IFormFile? Logo { get; set; }

        [Required(ErrorMessage = "Location is required")]
        [StringLength(500, ErrorMessage = "Location cannot exceed 500 characters")]
        public string Location { get; set; } = null!;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }
    }

    public class UpdateCompanyDto
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Phone is required")]
        [Phone(ErrorMessage = "Invalid phone format")]
        public string Phone { get; set; } = null!;

        [Url(ErrorMessage = "Invalid website URL format")]
        public string? Website { get; set; }

        
        public IFormFile? Logo { get; set; }

        [Required(ErrorMessage = "Location is required")]
        [StringLength(500, ErrorMessage = "Location cannot exceed 500 characters")]
        public string Location { get; set; } = null!;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }
    }
}