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
        private readonly UTCarreersContext _context;

        public CompaniesController(UTCarreersContext context)
        {
            _context = context;
        }

        // GET: api/Companies
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<CompanyDto>>>> GetCompanies()
        {
            try
            {
                var companies = await _context.Companies
                    .Where(c => c.DeletedAt == null)
                    .ToListAsync();
                var companyDtos = companies.Select(c => c.ToDto());
                return Ok(ApiResponse<IEnumerable<CompanyDto>>.SuccessResponse(companyDtos, "Companies retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<CompanyDto>>.ErrorResponse($"Internal server error: {ex.Message}"));
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
        public async Task<ActionResult<ApiResponse<CompanyDto>>> PutCompany(Guid id, UpdateCompanyDto updateDto)
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

                updateDto.UpdateModel(company);
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
        public async Task<ActionResult<ApiResponse<CompanyDto>>> PostCompany(CreateCompanyDto createDto)
        {
            try
            {
                var company = createDto.ToModel();
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

        private bool CompanyExists(Guid id)
        {
            return _context.Companies.Any(e => e.Id == id);
        }
    }
}
