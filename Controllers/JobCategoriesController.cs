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
    public class JobCategoriesController : ControllerBase
    {
        private readonly UTCareersContext _context;

        public JobCategoriesController(UTCareersContext context)
        {
            _context = context;
        }

        // GET: api/JobCategories
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<JobCategoryDto>>>> GetJobCategories(
            [FromQuery] int page = 1,
            [FromQuery] int per_page = 15)
        {
            try
            {
                var query = _context.JobCategories
                    .Where(jc => jc.DeletedAt == null);

                var totalCount = await query.CountAsync();
                
                var jobCategories = await query
                    .Skip((page - 1) * per_page)
                    .Take(per_page)
                    .ToListAsync();
                
                var jobCategoryDtos = jobCategories.Select(jc => jc.ToDto());
                
                var paginatedResponse = PaginationService.CreatePaginatedResponse(
                    jobCategoryDtos,
                    totalCount,
                    page,
                    per_page,
                    Request);
                
                return Ok(ApiResponse<PaginatedResponse<JobCategoryDto>>.SuccessResponse(paginatedResponse, "Job categories retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PaginatedResponse<JobCategoryDto>>.ErrorResponse($"Internal server error: {ex.Message}"));
            }
        }

        // GET: api/JobCategories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<JobCategoryDto>>> GetJobCategory(Guid id)
        {
            try
            {
                var jobCategory = await _context.JobCategories
                    .Where(jc => jc.Id == id && jc.DeletedAt == null)
                    .FirstOrDefaultAsync();

                if (jobCategory == null)
                {
                    return NotFound(ApiResponse<JobCategoryDto>.ErrorResponse("Job category not found"));
                }

                var jobCategoryDto = jobCategory.ToDto();
                return Ok(ApiResponse<JobCategoryDto>.SuccessResponse(jobCategoryDto, "Job category retrieved successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<JobCategoryDto>.ErrorResponse($"Internal server error: {ex.Message}"));
            }
        }

        // PUT: api/JobCategories/5
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<JobCategoryDto>>> PutJobCategory(Guid id, UpdateJobCategoryDto updateDto)
        {
            try
            {
                var jobCategory = await _context.JobCategories
                    .Where(jc => jc.Id == id && jc.DeletedAt == null)
                    .FirstOrDefaultAsync();

                if (jobCategory == null)
                {
                    return NotFound(ApiResponse<JobCategoryDto>.ErrorResponse("Job category not found"));
                }

                updateDto.UpdateModel(jobCategory);
                await _context.SaveChangesAsync();
                
                var jobCategoryDto = jobCategory.ToDto();
                return Ok(ApiResponse<JobCategoryDto>.SuccessResponse(jobCategoryDto, "Job category updated successfully"));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!JobCategoryExists(id))
                {
                    return NotFound(ApiResponse<JobCategoryDto>.ErrorResponse("Job category not found"));
                }
                else
                {
                    return StatusCode(500, ApiResponse<JobCategoryDto>.ErrorResponse("Concurrency error occurred"));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<JobCategoryDto>.ErrorResponse($"Internal server error: {ex.Message}"));
            }
        }

        // POST: api/JobCategories
        [HttpPost]
        public async Task<ActionResult<ApiResponse<JobCategoryDto>>> PostJobCategory(CreateJobCategoryDto createDto)
        {
            try
            {
                var jobCategory = createDto.ToModel();
                _context.JobCategories.Add(jobCategory);
                await _context.SaveChangesAsync();

                var jobCategoryDto = jobCategory.ToDto();
                return CreatedAtAction("GetJobCategory", new { id = jobCategory.Id }, 
                    ApiResponse<JobCategoryDto>.SuccessResponse(jobCategoryDto, "Job category created successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<JobCategoryDto>.ErrorResponse($"Internal server error: {ex.Message}"));
            }
        }

        // DELETE: api/JobCategories/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteJobCategory(Guid id)
        {
            try
            {
                var jobCategory = await _context.JobCategories
                    .Where(jc => jc.Id == id && jc.DeletedAt == null)
                    .FirstOrDefaultAsync();
                    
                if (jobCategory == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("Job category not found"));
                }

                // Soft delete
                jobCategory.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.SuccessResponse(null, "Job category deleted successfully"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse($"Internal server error: {ex.Message}"));
            }
        }

        private bool JobCategoryExists(Guid id)
        {
            return _context.JobCategories.Any(e => e.Id == id);
        }
    }
}
