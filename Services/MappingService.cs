using dotnet_utcareers.DTOs;
using dotnet_utcareers.Models;
using System.Security.Cryptography;
using System.Text;

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

        // User mappings
        public static UserDto ToDto(this User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Photo = user.Photo,
                Name = user.Name,
                Phone = user.Phone,
                Email = user.Email,
                Address = user.Address,
                Description = user.Description,
                Role = user.Role,
                VerifiedAt = user.VerifiedAt,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }

        public static User ToModel(this CreateUserDto dto)
        {
            return new User
            {
                Id = Guid.NewGuid(),
                Photo = dto.Photo,
                Name = dto.Name,
                Phone = dto.Phone,
                Email = dto.Email,
                Address = dto.Address,
                Description = dto.Description,
                Password = HashPassword(dto.Password),
                Role = dto.Role,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static void UpdateModel(this UpdateUserDto dto, User user)
        {
            user.Photo = dto.Photo;
            user.Name = dto.Name;
            user.Phone = dto.Phone;
            user.Email = dto.Email;
            user.Address = dto.Address;
            user.Description = dto.Description;
            if (!string.IsNullOrEmpty(dto.Role))
                user.Role = dto.Role;
            user.UpdatedAt = DateTime.UtcNow;
        }

        public static void ChangePassword(this ChangePasswordDto dto, User user)
        {
            user.Password = HashPassword(dto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            return HashPassword(password) == hashedPassword;
        }

        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
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
                Logo = company.Logo,
                Location = company.Location,
                Description = company.Description,
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
                Logo = dto.Logo?.FileName ?? "",
                Location = dto.Location,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static void UpdateModel(this UpdateCompanyDto dto, Company company)
        {
            if (dto.Logo != null)
                company.Logo = dto.Logo.FileName;
            if (dto.Name != null)
                company.Name = dto.Name;
            if (dto.Email != null)
                company.Email = dto.Email;
            if (dto.Phone != null)
                company.Phone = dto.Phone;
            if (dto.Website != null)
                company.Website = dto.Website;
            if (dto.Logo != null)
                company.Logo = dto.Logo.FileName;
            if (dto.Location != null)
                company.Location = dto.Location;
            if (dto.Description != null)
                company.Description = dto.Description;
            if (dto.Website != null)
                company.Website = dto.Website;
            company.UpdatedAt = DateTime.UtcNow;
        }

        // JobPost mappings
        public static JobPostDto ToDto(this JobPost jobPost)
        {
            var firstJobPostCategory = jobPost.JobPostCategories?.FirstOrDefault();
            return new JobPostDto
            {
                Id = jobPost.Id,
                CompanyId = jobPost.CompanyId,
                Title = jobPost.Title,
                Thumbnail = jobPost.Thumbnail,
                Status = jobPost.Status,
                CreatedAt = jobPost.CreatedAt,
                UpdatedAt = jobPost.UpdatedAt,
                Company = jobPost.Company?.ToDto(),
                JobCategories = jobPost.JobPostCategories?.Select(jpc => jpc.JobCategory?.ToDto()).Where(jc => jc != null).ToList()
            };
        }

        public static JobPost ToModel(this CreateJobPostDto dto)
        {
            return new JobPost
            {
                Id = Guid.NewGuid(),
                CompanyId = dto.CompanyId,
                Title = dto.Title,
                Thumbnail = dto.Thumbnail?.FileName ?? "",
                Status = dto.Status,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static void UpdateModel(this UpdateJobPostDto dto, JobPost jobPost)
        {
            jobPost.CompanyId = dto.CompanyId;
            jobPost.Title = dto.Title;
            if (dto.Thumbnail != null)
                jobPost.Thumbnail = dto.Thumbnail.FileName;
            jobPost.Status = dto.Status;
            jobPost.UpdatedAt = DateTime.UtcNow;
        }
    }
}