using dotnet_utcareers.DTOs;
using dotnet_utcareers.Models;

namespace dotnet_utcareers.Services
{
    public static class MappingService
    {
        // JobCategory mappings
        public static JobCategoryDto ToDto(this JobCategory jobCategory)
        {
            return new JobCategoryDto
            {
                Id = jobCategory.Id,
                Name = jobCategory.Name,
                CreatedAt = jobCategory.CreatedAt,
                UpdatedAt = jobCategory.UpdatedAt
            };
        }

        public static JobCategory ToModel(this CreateJobCategoryDto dto)
        {
            return new JobCategory
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static void UpdateModel(this UpdateJobCategoryDto dto, JobCategory jobCategory)
        {
            jobCategory.Name = dto.Name;
            jobCategory.UpdatedAt = DateTime.UtcNow;
        }

        // Company mappings
        public static CompanyDto ToDto(this Company company)
        {
            return new CompanyDto
            {
                Id = company.Id,
                Name = company.Name,
                Email = company.Email,
                Phone = company.Phone,
                Website = company.Website,
                Address = company.Address,
                CreatedAt = company.CreatedAt,
                UpdatedAt = company.UpdatedAt
            };
        }

        public static Company ToModel(this CreateCompanyDto dto)
        {
            return new Company
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                Website = dto.Website,
                Address = dto.Address,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static void UpdateModel(this UpdateCompanyDto dto, Company company)
        {
            company.Name = dto.Name;
            company.Email = dto.Email;
            company.Phone = dto.Phone;
            company.Website = dto.Website;
            company.Address = dto.Address;
            company.UpdatedAt = DateTime.UtcNow;
        }

        // JobPost mappings
        public static JobPostDto ToDto(this JobPost jobPost)
        {
            return new JobPostDto
            {
                Id = jobPost.Id,
                CompanyId = jobPost.CompanyId,
                Title = jobPost.Title,
                Description = jobPost.Description,
                Requirements = jobPost.Requirements,
                Benefits = jobPost.Benefits,
                Type = jobPost.Type,
                Status = jobPost.Status,
                CreatedAt = jobPost.CreatedAt,
                UpdatedAt = jobPost.UpdatedAt,
                Company = jobPost.Company?.ToDto(),
                JobCategories = jobPost.JobCategories?.Select(jc => jc.ToDto()).ToList()
            };
        }

        public static JobPost ToModel(this CreateJobPostDto dto)
        {
            return new JobPost
            {
                Id = Guid.NewGuid(),
                CompanyId = dto.CompanyId,
                Title = dto.Title,
                Description = dto.Description,
                Requirements = dto.Requirements,
                Benefits = dto.Benefits,
                Type = dto.Type,
                Status = dto.Status,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static void UpdateModel(this UpdateJobPostDto dto, JobPost jobPost)
        {
            jobPost.CompanyId = dto.CompanyId;
            jobPost.Title = dto.Title;
            jobPost.Description = dto.Description;
            jobPost.Requirements = dto.Requirements;
            jobPost.Benefits = dto.Benefits;
            jobPost.Type = dto.Type;
            jobPost.Status = dto.Status;
            jobPost.UpdatedAt = DateTime.UtcNow;
        }
    }
}