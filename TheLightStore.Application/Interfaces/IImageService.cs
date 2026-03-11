using TheLightStore.Application.DTOs.Images;

namespace TheLightStore.Application.Interfaces;

public interface IImageService
{
    string GetFullImageUrl(string relativePath);
    Task<string> SaveImageAsync(ImageUploadDto imageDto, string folder = "products", int? productId = null);
    Task<bool> DeleteImageAsync(string imageUrl);
    bool IsValidImageFile(string fileName, string contentType, long fileSize);
}
