using Microsoft.AspNetCore.Http;

namespace testAPI.api.application.ServiceInterfaces
{
    public interface IFileService
    {
        Task<string?> SaveImageAsync(IFormFile imageFile, string folderName);
        Task<bool> DeleteImageAsync(string imagePath);
        Task<List<string>> SaveMultipleImagesAsync(List<IFormFile> imageFiles, string folderName);
        bool IsValidImage(IFormFile imageFile);
        string GetImageMimeType(string fileName);
    }
}
