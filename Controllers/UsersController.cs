using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using dotnet_utcareers.Data;
using dotnet_utcareers.Models;
using dotnet_utcareers.DTOs;
using dotnet_utcareers.Services;

namespace dotnet_utcareers.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UTCareersContext _context;
        private readonly ImageUploadService _imageUploadService;

        public UsersController(UTCareersContext context, ImageUploadService imageUploadService)
        {
            _context = context;
            _imageUploadService = imageUploadService;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<UserDto>>>> GetUsers(
            [FromQuery] int page = 1,
            [FromQuery] int per_page = 15,
            [FromQuery] string search = null)
        {
            try
            {
                var query = _context.Users.Where(u => u.DeletedAt == null && u.Role == "pelamar");

                // Apply search filter if search parameter is provided
                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.ToLower();
                    query = query.Where(u =>
                        u.Name.ToLower().Contains(search) ||
                        u.Email.ToLower().Contains(search)
                    );
                }

                var totalCount = await query.CountAsync();

                var users = await query
                    .Skip((page - 1) * per_page)
                    .Where((t) => t.DeletedAt == null)
                    .Take(per_page)
                    .ToListAsync();

                var userDtos = users.Select(u => u.ToDto());

                var paginatedResponse = PaginationService.CreatePaginatedResponse(
                    userDtos,
                    totalCount,
                    page,
                    per_page,
                    Request);

                return Ok(ApiResponse<PaginatedResponse<UserDto>>.SuccessResponse(paginatedResponse, "Data users berhasil diambil"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PaginatedResponse<UserDto>>.ErrorResponse($"Internal server error: {ex.Message}"));
            }
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound(ApiResponse<UserDto>.ErrorResponse("User tidak ditemukan"));
            }

            return Ok(ApiResponse<UserDto>.SuccessResponse(user.ToDto(), "Data user berhasil diambil"));
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<UserDto>>> PutUser(Guid id, [FromForm] UpdateUserDto updateDto)
        {
            try
            {
                var user = await _context.Users
                    .Where(u => u.Id == id && u.DeletedAt == null)
                    .FirstOrDefaultAsync();

                var checkEmailAndPhoneNumber = await _context.Users
                .Where(u => u.Id != id && (u.Email == updateDto.Email || u.Phone == updateDto.Phone) && u.DeletedAt == null)
                .FirstAsync();

                if (checkEmailAndPhoneNumber != null)
                {
                    return BadRequest(ApiResponse<UserDto>.ErrorResponse("Email atau nomor telp sudah digunakan"));
                }


                if (user == null)
                {
                    return NotFound(ApiResponse<UserDto>.ErrorResponse("User tidak ditemukan"));
                }

                // Update user properties (excluding role)
                user.Name = updateDto.Name;
                user.Phone = updateDto.Phone;
                user.Email = updateDto.Email;
                user.Address = updateDto.Address;
                user.Description = updateDto.Description;
                user.UpdatedAt = DateTime.UtcNow;
                // If password is provided, hash and update it
                if (!string.IsNullOrEmpty(updateDto.Password))
                {
                    user.Password = MappingService.HashPassword(updateDto.Password);
                }

                // Handle photo upload if provided
                if (updateDto.Photo != null)
                {
                    // Delete old photo if exists
                    if (!string.IsNullOrEmpty(user.Photo))
                    {
                        await _imageUploadService.DeleteImageAsync(user.Photo);
                    }

                    var photoUrl = await _imageUploadService.UploadImageAsync(updateDto.Photo, "users");
                    user.Photo = photoUrl;
                }

                await _context.SaveChangesAsync();

                var userDto = user.ToDto();
                return Ok(ApiResponse<UserDto>.SuccessResponse(userDto, "Data user berhasil diperbarui"));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound(ApiResponse<UserDto>.ErrorResponse("User tidak ditemukan"));
                }
                else
                {
                    return StatusCode(500, ApiResponse<UserDto>.ErrorResponse("Concurrency error occurred"));
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<UserDto>.ErrorResponse($"Internal server error: {ex.Message}"));
            }
        }

        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<ApiResponse<UserDto>>> PostUser([FromForm] CreateUserDto createDto)
        {
            try
            {
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Name = createDto.Name,
                    Phone = createDto.Phone,
                    Email = createDto.Email,
                    Address = createDto.Address,
                    Description = createDto.Description,
                    Password = MappingService.HashPassword(createDto.Password),
                    Role = "pelamar",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Handle photo upload if provided
                if (createDto.Photo != null)
                {
                    var photoUrl = await _imageUploadService.UploadImageAsync(createDto.Photo, "users");
                    user.Photo = photoUrl;
                }

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var userDto = user.ToDto();
                return CreatedAtAction("GetUser", new { id = user.Id },
                    ApiResponse<UserDto>.SuccessResponse(userDto, "User berhasil dibuat"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<UserDto>.ErrorResponse($"Internal server error: {ex.Message}"));
            }
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("User tidak ditemukan"));
            }

            // Delete photo from S3 if exists
            if (!string.IsNullOrEmpty(user.Photo))
            {
                await _imageUploadService.DeleteImageAsync(user.Photo);
            }
            user.DeletedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessResponse(null, "User berhasil dihapus"));
        }

        // POST: api/Users/5/change-password
        [HttpPost("{id}/change-password")]
        public async Task<IActionResult> ChangePassword(Guid id, ChangePasswordDto changePasswordDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("User tidak ditemukan"));
            }

            if (!MappingService.VerifyPassword(changePasswordDto.CurrentPassword, user.Password))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Password saat ini tidak sesuai"));
            }

            user.Password = MappingService.HashPassword(changePasswordDto.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessResponse(null, "Password berhasil diubah"));
        }

        // POST: api/Users/5/upload-photo
        [HttpPost("{id}/upload-photo")]
        public async Task<IActionResult> UploadPhoto(Guid id, IFormFile photo)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("User tidak ditemukan"));
            }

            try
            {
                // Delete old photo if exists
                if (!string.IsNullOrEmpty(user.Photo))
                {
                    await _imageUploadService.DeleteImageAsync(user.Photo);
                }

                // Upload new photo
                var photoUrl = await _imageUploadService.UploadImageAsync(photo, "users");
                user.Photo = photoUrl;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.SuccessResponse(new { PhotoUrl = photoUrl }, "Foto berhasil diupload"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.ErrorResponse($"Error uploading photo: {ex.Message}"));
            }
        }

        private bool UserExists(Guid id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}