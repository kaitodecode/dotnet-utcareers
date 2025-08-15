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
        private readonly UTCareersContext _context;
        private readonly ImageUploadService _imageUploadService;

        public JobPostsController(UTCareersContext context, ImageUploadService imageUploadService)
        {
            _context = context;
            _imageUploadService = imageUploadService;
        }

        // GET: api/JobPosts
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<JobPostDto>>>> GetJobPosts(
            [FromQuery] int page = 1,
            [FromQuery] int per_page = 15,
            [FromQuery] string search = null)
        {
            try
            {
                var query = _context.JobPosts
                    .Include(jp => jp.Company)
                    .Include(jp => jp.JobPostCategories)
                        .ThenInclude(jpc => jpc.JobCategory)
                    .Where(jp => jp.DeletedAt == null);

                // Apply search filter if search parameter is provided
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.ToLower();
                    query = query.Where(jp =>
                        jp.Title.ToLower().Contains(search) ||
                        jp.Company.Name.ToLower().Contains(search)
                    );
                }

                var totalCount = await query.CountAsync();
                
                var jobPosts = await query
                    .Skip((page - 1) * per_page)
                    .Take(per_page)
                    .ToListAsync();
                
                var jobPostDtos = jobPosts.Select(jp => jp.ToDto());
                
                var paginatedResponse = PaginationService.CreatePaginatedResponse(
                    jobPostDtos,
                    totalCount,
                    page,
                    per_page,
                    Request);
                
                return Ok(ApiResponse<PaginatedResponse<JobPostDto>>.SuccessResponse(paginatedResponse, "Data job posts berhasil diambil"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PaginatedResponse<JobPostDto>>.ErrorResponse($"Terjadi kesalahan: {ex.Message}"));
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
                    .Include(jp => jp.JobPostCategories)
                        .ThenInclude(jpc => jpc.JobCategory)
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
        public async Task<ActionResult<ApiResponse<JobPostDto>>> PutJobPost(Guid id, [FromForm] UpdateJobPostDto updateDto)
        {
            try
            {
                var jobPost = await _context.JobPosts
                    .Where(jp => jp.Id == id && jp.DeletedAt == null)
                    .FirstOrDefaultAsync();

                if (jobPost == null)
                {
                    return NotFound(ApiResponse<JobPostDto>.ErrorResponse("Job post tidak ditemukan"));
                }

                // Handle thumbnail update if provided
                if (updateDto.Thumbnail != null)
                {
                    // Delete old thumbnail if exists
                    if (!string.IsNullOrEmpty(jobPost.Thumbnail))
                    {
                        await _imageUploadService.DeleteImageAsync(jobPost.Thumbnail);
                    }

                    // Upload new thumbnail
                    var thumbnailUrl = await _imageUploadService.UploadImageAsync(updateDto.Thumbnail, "jobposts");
                    jobPost.Thumbnail = thumbnailUrl;
                }


                // Update basic properties
                jobPost.Title = updateDto.Title;
                jobPost.Status = updateDto.Status;
                jobPost.UpdatedAt = DateTime.UtcNow;

                // Update job categories
                if (updateDto.JobCategories != null && updateDto.JobCategories.Count > 0)
                {
                    var jobCategories = updateDto.JobCategories;

                    // Get existing categories that are not in the update
                    var newCategoryIds = jobCategories.Select(c => c.JobCategoryId).ToList();
                    var categoriesToRemove = jobPost.JobPostCategories
                        .Where(jpc => !newCategoryIds.Contains(jpc.JobCategoryId))
                        .ToList();

                    // Remove only categories that are no longer needed
                    if (categoriesToRemove.Any())
                    {
                        _context.JobPostCategories.RemoveRange(categoriesToRemove);
                    }
                    
                    // Add or update categories
                    foreach (var categoryDto in jobCategories)
                    {
                        var existingCategory = jobPost.JobPostCategories
                            .FirstOrDefault(jpc => jpc.JobCategoryId == categoryDto.JobCategoryId);

                        if (existingCategory != null)
                        {
                            // Update existing category
                            existingCategory.Type = categoryDto.Type;
                            existingCategory.RequiredCount = categoryDto.RequiredCount;
                            existingCategory.Description = categoryDto.Description;
                            existingCategory.Requirements = categoryDto.Requirements;
                            existingCategory.Benefits = categoryDto.Benefits;
                            existingCategory.UpdatedAt = DateTime.UtcNow;
                        }
                        else
                        {
                            // Add new category
                            jobPost.JobPostCategories.Add(new JobPostCategory
                            {
                                Id = Guid.NewGuid(),
                                JobCategoryId = categoryDto.JobCategoryId,
                                JobPostId = jobPost.Id,
                                Type = categoryDto.Type,
                                RequiredCount = categoryDto.RequiredCount,
                                Description = categoryDto.Description,
                                Requirements = categoryDto.Requirements,
                                Benefits = categoryDto.Benefits,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            });
                        }
                    }
                }
                else
                {
                    return BadRequest(ApiResponse<JobPostDto>.ErrorResponse("At least one job category is required"));
                }

                await _context.SaveChangesAsync();
                
                // Reload with includes for DTO conversion
                await _context.Entry(jobPost)
                    .Collection(jp => jp.JobPostCategories)
                    .Query()
                    .Include(jpc => jpc.JobCategory)
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
        public async Task<ActionResult<ApiResponse<JobPostDto>>> PostJobPost([FromForm] CreateJobPostDto createDto)
        {
            try
            {
                Console.WriteLine($"JobCategories is null: {createDto.JobCategories == null}");
                if (createDto.JobCategories != null)
                {
                    Console.WriteLine($"JobCategories count: {createDto.JobCategories.Count}");
                    for (int i = 0; i < createDto.JobCategories.Count; i++)
                    {
                        var cat = createDto.JobCategories[i];
                        Console.WriteLine($"Category {i}: JobCategoryId={cat.JobCategoryId}, Type={cat.Type}, RequiredCount={cat.RequiredCount}");
                    }
                }
                
                // Validate JobCategories
                if (createDto.JobCategories == null || createDto.JobCategories.Count == 0)
                {
                    return BadRequest(ApiResponse<JobPostDto>.ErrorResponse("At least one job category is required"));
                }
                
                var jobCategories = createDto.JobCategories;

                // Upload thumbnail
                var thumbnailUrl = await _imageUploadService.UploadImageAsync(createDto.Thumbnail, "jobposts");

                var jobPost = new JobPost
                {
                    Id = Guid.NewGuid(),
                    CompanyId = createDto.CompanyId,
                    Title = createDto.Title,
                    Thumbnail = thumbnailUrl,
                    Status = createDto.Status,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.JobPosts.Add(jobPost);

                // Add job categories
                foreach (var categoryDto in jobCategories)
                {
                    jobPost.JobPostCategories.Add(new JobPostCategory
                    {
                        Id = Guid.NewGuid(),
                        JobCategoryId = categoryDto.JobCategoryId,
                        JobPostId = jobPost.Id,
                        Type = categoryDto.Type,
                        RequiredCount = categoryDto.RequiredCount,
                        Description = categoryDto.Description,
                        Requirements = categoryDto.Requirements,
                        Benefits = categoryDto.Benefits,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }
                
                await _context.SaveChangesAsync();

                // Load includes for DTO conversion
                await _context.Entry(jobPost)
                    .Collection(jp => jp.JobPostCategories)
                    .Query()
                    .Include(jpc => jpc.JobCategory)
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

        // POST: api/JobPosts/5/upload-thumbnail
        [HttpPost("{id}/upload-thumbnail")]
        public async Task<IActionResult> UploadThumbnail(Guid id, IFormFile thumbnail)
        {
            var jobPost = await _context.JobPosts.FindAsync(id);
            if (jobPost == null)
            {
                return NotFound();
            }

            try
            {
                // Delete old thumbnail if exists
                if (!string.IsNullOrEmpty(jobPost.Thumbnail))
                {
                    await _imageUploadService.DeleteImageAsync(jobPost.Thumbnail);
                }

                // Upload new thumbnail
                var thumbnailUrl = await _imageUploadService.UploadImageAsync(thumbnail, "jobposts");
                jobPost.Thumbnail = thumbnailUrl;
                jobPost.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { ThumbnailUrl = thumbnailUrl });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error uploading thumbnail: {ex.Message}");
            }
        }

        private bool JobPostExists(Guid id)
        {
            return _context.JobPosts.Any(e => e.Id == id);
        }
    }
}
