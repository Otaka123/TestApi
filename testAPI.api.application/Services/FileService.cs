using testAPI.api.application.ServiceInterfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace testAPI.api.application.Services
{
    public class FileService : IFileService
    {
        private readonly IHostEnvironment _environment;
        private readonly ILogger<FileService> _logger;

        public FileService(IHostEnvironment environment, ILogger<FileService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<string?> SaveImageAsync(IFormFile imageFile, string folderName)
        {
            try
            {
                if (imageFile == null || imageFile.Length == 0)
                    return null;

                if (!IsValidImage(imageFile))
                    return null;

                var contentRootPath = _environment.ContentRootPath;
                var webRootPath = Path.Combine(contentRootPath, "wwwroot");
                var uploadsFolder = Path.Combine(webRootPath, "images", folderName);

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                    await imageFile.CopyToAsync(fileStream);

                return Path.Combine("images", folderName, fileName).Replace("\\", "/");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving image");
                return null;
            }
        }

        public async Task<bool> DeleteImageAsync(string imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                    return false;

                var fullPath = Path.Combine(_environment.ContentRootPath, "wwwroot", imagePath);

                if (!File.Exists(fullPath))
                    return false;

                File.Delete(fullPath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image");
                return false;
            }
        }

        public async Task<List<string>> SaveMultipleImagesAsync(List<IFormFile> imageFiles, string folderName)
        {
            var savedPaths = new List<string>();

            foreach (var imageFile in imageFiles)
            {
                var savedPath = await SaveImageAsync(imageFile, folderName);
                if (!string.IsNullOrEmpty(savedPath))
                    savedPaths.Add(savedPath);
            }

            return savedPaths;
        }

        public bool IsValidImage(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return false;

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".pdf" };
            var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

            if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
                return false;

            const long maxFileSize = 10 * 1024 * 1024;
            if (imageFile.Length > maxFileSize)
                return false;

            var allowedMimeTypes = new[]
            {
                "image/jpeg", "image/jpg", "image/png", "image/gif",
                "image/bmp", "image/webp", "application/pdf"
            };

            return allowedMimeTypes.Contains(imageFile.ContentType.ToLowerInvariant());
        }

        public string GetImageMimeType(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                ".pdf" => "application/pdf",
                _ => "application/octet-stream"
            };
        }
    }
}
