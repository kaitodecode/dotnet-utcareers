using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using dotnet_utcareers.Data;
using dotnet_utcareers.Models;
using dotnet_utcareers.DTOs;
using dotnet_utcareers.Services;
using Microsoft.AspNetCore.Authorization;

namespace dotnet_utcareers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CompaniesController : ControllerBase
    {
        private readonly UTCareersContext _context;
        private readonly ImageUploadService _imageUploadService;

        public CompaniesController(UTCareersContext context, ImageUploadService imageUploadService)
        {
            _context = context;
            _imageUploadService = imageUploadService;
        }

        // GET: api/Companies
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<CompanyDto>>>> GetCompanies(
            [FromQuery] int page = 1,
            [FromQuery] int per_page = 15)
        {
            try
            {
                var query = _context.Companies
                    .Where(c => c.DeletedAt == null);

                var totalCount = await query.CountAsync();
                
                var companies = await query
                    .Skip((page - 1) * per_page)
                    .Take(per_page)
                    .ToListAsync();
                
                var companyDtos = companies.Select(c => c.ToDto());
                
                var paginatedResponse = PaginationService.CreatePaginatedResponse(
                    companyDtos,
                    totalCount,
                    page,
                    per_page,
                    Request);
                
                return Ok(ApiResponse<PaginatedResponse<CompanyDto>>.SuccessResponse(paginatedResponse, "Companies retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PaginatedResponse<CompanyDto>>.ErrorResponse($"Internal server error: {ex.Message}"));
            }
        }

        // GET: api/Companies/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<CompanyDto>>> GetCompany(Guid id)
        {
            try
            {
                var company = await _context.Companies
                    .Where(c => c.Id == id && c.DeletedAt == null)
                    .FirstOrDefaultAsync();

                if (company == null)
                {
                    return NotFound(ApiResponse<CompanyDto>.ErrorResponse("Company not found"));
                }

                var companyDto = company.ToDto();
                return Ok(ApiResponse<CompanyDto>.SuccessResponse(companyDto, "Company retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CompanyDto>.ErrorResponse($"Internal server error: {ex.Message}"));
            }
        }

        // PUT: api/Companies/5
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<CompanyDto>>> PutCompany(Guid id, [FromForm] UpdateCompanyDto updateDto)
        {
            try
            {
                var company = await _context.Companies
                    .Where(c => c.Id == id && c.DeletedAt == null)
                    .FirstOrDefaultAsync();

                if (company == null)
                {
                    return NotFound(ApiResponse<CompanyDto>.ErrorResponse("Company not found"));
                }

                // Update company properties
                company.Name = updateDto.Name;
                company.Description = updateDto.Description;
                company.Email = updateDto.Email;
                company.Phone = updateDto.Phone;
                company.Location = updateDto.Location;
                company.Website = updateDto.Website;
                company.UpdatedAt = DateTime.UtcNow;

                // Handle logo upload if provided
                if (updateDto.Logo != null)
                {
                    // Delete old logo if exists
                    if (!string.IsNullOrEmpty(company.Logo))
                    {
                        await _imageUploadService.DeleteImageAsync(company.Logo);
                    }
                    
                    var logoUrl = await _imageUploadService.UploadImageAsync(updateDto.Logo, "companies");
                    company.Logo = logoUrl;
                }

                await _context.SaveChangesAsync();
                
                var companyDto = company.ToDto();
                return Ok(ApiResponse<CompanyDto>.SuccessResponse(companyDto, "Company updated successfully"));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CompanyExists(id))
                {
                    return NotFound(ApiResponse<CompanyDto>.ErrorResponse("Company not found"));
                }
                else
                {
                    return StatusCode(500, ApiResponse<CompanyDto>.ErrorResponse("Concurrency error occurred"));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CompanyDto>.ErrorResponse($"Internal server error: {ex.Message}"));
            }
        }

        // POST: api/Companies
        [HttpPost]
        public async Task<ActionResult<ApiResponse<CompanyDto>>> PostCompany([FromForm] CreateCompanyDto createDto)
        {
            try
            {

                var company = new Company
                {
                    Id = Guid.NewGuid(),
                    Name = createDto.Name,
                    Description = createDto.Description,
                    Email = createDto.Email,
                    Phone = createDto.Phone,
                    Location = createDto.Location,
                    Website = createDto.Website,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };

                if (createDto.Logo != null)
                {
                    var logoUrl = await _imageUploadService.UploadImageAsync(createDto.Logo, "companies");
                    company.Logo = logoUrl;
                }


                _context.Companies.Add(company);
                await _context.SaveChangesAsync();

                var companyDto = company.ToDto();
                return CreatedAtAction("GetCompany", new { id = company.Id }, 
                    ApiResponse<CompanyDto>.SuccessResponse(companyDto, "Company created successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<CompanyDto>.ErrorResponse($"Internal server error: {ex.Message}"));
            }
        }

        // DELETE: api/Companies/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteCompany(Guid id)
        {
            try
            {
                var company = await _context.Companies
                    .Where(c => c.Id == id && c.DeletedAt == null)
                    .FirstOrDefaultAsync();
                    
                if (company == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("Company not found"));
                }

                // Soft delete
                company.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.SuccessResponse(null, "Company deleted successfully"));
            }
            catch (Exception ex)
            {
                 return StatusCode(500, ApiResponse<object>.ErrorResponse($"Internal server error: {ex.Message}"));
             }
         }

        // POST: api/Companies/5/upload-logo
        [HttpPost("{id}/upload-logo")]
        public async Task<IActionResult> UploadLogo(Guid id, IFormFile logo)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
                return NotFound();
            }

            try
            {
                // Delete old logo if exists
                if (!string.IsNullOrEmpty(company.Logo))
                {
                    await _imageUploadService.DeleteImageAsync(company.Logo);
                }

                // Upload new logo
                var logoUrl = await _imageUploadService.UploadImageAsync(logo, "companies");
                company.Logo = logoUrl;
                company.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { LogoUrl = logoUrl });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error uploading logo: {ex.Message}");
            }
        }

        private bool CompanyExists(Guid id)
        {
            return _context.Companies.Any(e => e.Id == id);
        }
    }
}
