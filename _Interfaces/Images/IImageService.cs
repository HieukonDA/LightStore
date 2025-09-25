namespace TheLightStore.Interfaces.Images;

public interface IImageService
{
    string GetFullImageUrl(string relativePath);
    Task<string> SaveImageAsync(IFormFile file, string folder = "products", int? productId = null);
    Task<bool> DeleteImageAsync(string imageUrl);
    bool IsValidImageFile(IFormFile file);
}