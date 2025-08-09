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
    public class JobPostsController : ControllerBase
    {
        private readonly UTCarreersContext _context;

        public JobPostsController(UTCarreersContext context)
        {
            _context = context;
        }

        // GET: api/JobPosts
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<JobPostDto>>>> GetJobPosts()
        {
            try
            {
                var jobPosts = await _context.JobPosts
                    .Include(jp => jp.Company)
                    .Include(jp => jp.JobCategories)
                    .Where(jp => jp.DeletedAt == null)
                    .ToListAsync();
                var jobPostDtos = jobPosts.Select(jp => jp.ToDto());
                return Ok(ApiResponse<IEnumerable<JobPostDto>>.SuccessResponse(jobPostDtos, "Data job posts berhasil diambil"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<JobPostDto>>.ErrorResponse($"Terjadi kesalahan: {ex.Message}"));
            }
        }

        // GET: api/JobPosts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<JobPostDto>>> GetJobPost(Guid id)
        {
            try
            {
                var jobPost = await _context.JobPosts
                    .Include(jp => jp.Company)
                    .Include(jp => jp.JobCategories)
                    .Where(jp => jp.Id == id && jp.DeletedAt == null)
                    .FirstOrDefaultAsync();

                if (jobPost == null)
                {
                    return NotFound(ApiResponse<JobPostDto>.ErrorResponse("Job post tidak ditemukan"));
                }

                var jobPostDto = jobPost.ToDto();
                return Ok(ApiResponse<JobPostDto>.SuccessResponse(jobPostDto, "Data job post berhasil diambil"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<JobPostDto>.ErrorResponse($"Terjadi kesalahan: {ex.Message}"));
            }
        }

        // PUT: api/JobPosts/5
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<JobPostDto>>> PutJobPost(Guid id, UpdateJobPostDto updateDto)
        {
            try
            {
                var jobPost = await _context.JobPosts
                    .Include(jp => jp.JobCategories)
                    .Where(jp => jp.Id == id && jp.DeletedAt == null)
                    .FirstOrDefaultAsync();

                if (jobPost == null)
                {
                    return NotFound(ApiResponse<JobPostDto>.ErrorResponse("Job post tidak ditemukan"));
                }

                // Update job categories if provided
                if (updateDto.JobCategoryIds != null && updateDto.JobCategoryIds.Any())
                {
                    var jobCategories = await _context.JobCategories
                        .Where(jc => updateDto.JobCategoryIds.Contains(jc.Id))
                        .ToListAsync();
                    jobPost.JobCategories = jobCategories;
                }

                updateDto.UpdateModel(jobPost);
                await _context.SaveChangesAsync();
                
                // Reload with includes for DTO conversion
                await _context.Entry(jobPost)
                    .Collection(jp => jp.JobCategories)
                    .LoadAsync();
                await _context.Entry(jobPost)
                    .Reference(jp => jp.Company)
                    .LoadAsync();
                
                var jobPostDto = jobPost.ToDto();
                return Ok(ApiResponse<JobPostDto>.SuccessResponse(jobPostDto, "Job post berhasil diupdate"));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!JobPostExists(id))
                {
                    return NotFound(ApiResponse<JobPostDto>.ErrorResponse("Job post tidak ditemukan"));
                }
                else
                {
                    return StatusCode(500, ApiResponse<JobPostDto>.ErrorResponse("Terjadi konflik saat mengupdate data"));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<JobPostDto>.ErrorResponse($"Terjadi kesalahan: {ex.Message}"));
            }
        }

        // POST: api/JobPosts
        [HttpPost]
        public async Task<ActionResult<ApiResponse<JobPostDto>>> PostJobPost(CreateJobPostDto createDto)
        {
            try
            {
                var jobPost = createDto.ToModel();
                
                // Add job categories if provided
                if (createDto.JobCategoryIds != null && createDto.JobCategoryIds.Any())
                {
                    var jobCategories = await _context.JobCategories
                        .Where(jc => createDto.JobCategoryIds.Contains(jc.Id))
                        .ToListAsync();
                    jobPost.JobCategories = jobCategories;
                }
                
                _context.JobPosts.Add(jobPost);
                await _context.SaveChangesAsync();

                // Load includes for DTO conversion
                await _context.Entry(jobPost)
                    .Collection(jp => jp.JobCategories)
                    .LoadAsync();
                await _context.Entry(jobPost)
                    .Reference(jp => jp.Company)
                    .LoadAsync();
                
                var jobPostDto = jobPost.ToDto();
                var response = ApiResponse<JobPostDto>.SuccessResponse(jobPostDto, "Job post berhasil dibuat");
                return CreatedAtAction("GetJobPost", new { id = jobPost.Id }, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<JobPostDto>.ErrorResponse($"Terjadi kesalahan: {ex.Message}"));
            }
        }

        // DELETE: api/JobPosts/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteJobPost(Guid id)
        {
            try
            {
                var jobPost = await _context.JobPosts
                    .Where(jp => jp.Id == id && jp.DeletedAt == null)
                    .FirstOrDefaultAsync();
                    
                if (jobPost == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("Job post tidak ditemukan"));
                }

                // Soft delete
                jobPost.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.SuccessResponse(null, "Job post berhasil dihapus"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse($"Terjadi kesalahan: {ex.Message}"));
            }
        }

        private bool JobPostExists(Guid id)
        {
            return _context.JobPosts.Any(e => e.Id == id);
        }
    }
}
