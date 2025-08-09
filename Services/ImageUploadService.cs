using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace dotnet_utcareers.Services
{
    public interface IImageUploadService
    {
        Task<string> UploadImageAsync(IFormFile file, string folder = "images");
        Task<bool> DeleteImageAsync(string imageUrl);
        bool IsValidImageFile(IFormFile file);
    }

    public class ImageUploadService : IImageUploadService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly IConfiguration _configuration;
        private readonly string _bucketName;
        private readonly string _region;
        private readonly string _baseUrl;
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        public ImageUploadService(IAmazonS3 s3Client, IConfiguration configuration)
        {
            _s3Client = s3Client;
            _configuration = configuration;
            _bucketName = _configuration["AWS:BucketName"] ?? throw new ArgumentNullException("AWS:BucketName not configured");
            _region = _configuration["AWS:Region"] ?? "ap-southeast-1";
            var serviceUrl = _configuration["AWS:ServiceURL"] ?? "https://is3.cloudhost.id";
            _baseUrl = $"{serviceUrl}/{_bucketName}";
        }

        public bool IsValidImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            if (file.Length > _maxFileSize)
                return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return _allowedExtensions.Contains(extension);
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folder = "images")
        {
            if (!IsValidImageFile(file))
                throw new ArgumentException("Invalid image file. Allowed formats: JPG, JPEG, PNG, GIF, WEBP. Max size: 5MB.");

            try
            {
                var fileName = GenerateUniqueFileName(file.FileName);
                var key = $"{folder}/{fileName}";

                using var stream = file.OpenReadStream();
                
                var request = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key,
                    InputStream = stream,
                    ContentType = GetContentType(file.FileName),
                    CannedACL = S3CannedACL.PublicRead
                };

                var response = await _s3Client.PutObjectAsync(request);
                
                if (response.HttpStatusCode == HttpStatusCode.OK)
                {
                    return $"{_baseUrl}/{key}";
                }
                
                throw new Exception($"Failed to upload image. Status: {response.HttpStatusCode}");
            }
            catch (AmazonS3Exception ex)
            {
                throw new Exception($"S3 Error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Upload failed: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl) || !imageUrl.StartsWith(_baseUrl))
                return false;

            try
            {
                var key = imageUrl.Replace($"{_baseUrl}/", "");
                
                var request = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key
                };

                var response = await _s3Client.DeleteObjectAsync(request);
                return response.HttpStatusCode == HttpStatusCode.NoContent;
            }
            catch (AmazonS3Exception ex)
            {
                // Log error but don't throw - file might already be deleted
                Console.WriteLine($"S3 Delete Error: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Delete failed: {ex.Message}");
                return false;
            }
        }

        private string GenerateUniqueFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var guid = Guid.NewGuid().ToString("N")[..8];
            return $"{timestamp}_{guid}{extension}";
        }

        private string GetContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }
    }
}