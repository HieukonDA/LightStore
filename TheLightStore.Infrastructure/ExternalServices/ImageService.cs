using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TheLightStore.Application.DTOs.Images;
using TheLightStore.Application.Interfaces;

namespace TheLightStore.Infrastructure.ExternalServices;

public class ImageService : IImageService
{
    private readonly IFileStorageService _fileStorage;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ImageService> _logger;
    
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB

    public ImageService(
        IFileStorageService fileStorage, 
        IConfiguration configuration,
        ILogger<ImageService> logger)
    {
        _fileStorage = fileStorage;
        _configuration = configuration;
        _logger = logger;
    }

    public string GetFullImageUrl(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
            return string.Empty;
            
        var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "http://localhost:5000";
        
        // Nếu đã là full URL thì return luôn
        if (relativePath.StartsWith("http"))
            return relativePath;
            
        // Nếu không bắt đầu bằng / thì thêm vào
        if (!relativePath.StartsWith("/"))
            relativePath = "/" + relativePath;
            
        return $"{baseUrl}{relativePath}";
    }

    public async Task<string> SaveImageAsync(ImageUploadDto imageDto, string folder = "product", int? productId = null)
    {
        if (!IsValidImageFile(imageDto.FileName, imageDto.ContentType, imageDto.FileSize))
            throw new ArgumentException("Invalid image file");

        string uploadDir;
        string relativeUrl;

        if (productId.HasValue)
        {
            // Lưu theo cấu trúc product/productX/
            var productFolder = $"product{productId}";
            var relativePath = Path.Combine(folder, productFolder);
            uploadDir = Path.Combine(_fileStorage.GetWebRootPath(), relativePath);
            
            Directory.CreateDirectory(uploadDir);

            // Tìm số thứ tự tiếp theo
            var existingFiles = Directory.GetFiles(uploadDir, "*.*")
                .Where(f => _allowedExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .ToList();

            int nextNumber = 1;
            if (existingFiles.Any())
            {
                var numbers = existingFiles.Select(f => 
                {
                    var fileName = Path.GetFileNameWithoutExtension(f);
                    if (int.TryParse(fileName, out int num))
                        return num;
                    return 0;
                }).Where(n => n > 0);
                
                if (numbers.Any())
                {
                    nextNumber = numbers.Max() + 1;
                }
            }

            var fileExtension = Path.GetExtension(imageDto.FileName).ToLowerInvariant();
            var fileName = $"{nextNumber}{fileExtension}";
            var filePath = Path.Combine(uploadDir, fileName);
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageDto.FileStream.CopyToAsync(stream);
            }
            
            relativeUrl = $"/{relativePath.Replace("\\", "/")}/{fileName}";
        }
        else
        {
            // Fallback: lưu với GUID như cũ
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageDto.FileName)}";
            uploadDir = Path.Combine(_fileStorage.GetWebRootPath(), folder);
            Directory.CreateDirectory(uploadDir);
            
            var filePath = Path.Combine(uploadDir, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageDto.FileStream.CopyToAsync(stream);
            }
            
            relativeUrl = $"/{folder}/{fileName}";
        }
        
        return relativeUrl;
    }

    public async Task<bool> DeleteImageAsync(string imageUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(imageUrl))
                return false;

            // Xử lý cả URL cũ và mới  
            string relativePath;
            if (imageUrl.StartsWith("/product/"))
            {
                // URL mới: /product/product1/1.png
                relativePath = imageUrl.TrimStart('/');
            }
            else if (imageUrl.StartsWith("/uploads/"))
            {
                // URL cũ: /uploads/products/2024/12/guid.png
                relativePath = imageUrl.TrimStart('/');
            }
            else
            {
                return false;
            }

            var filePath = Path.Combine(_fileStorage.GetWebRootPath(), relativePath);
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image: {ImageUrl}", imageUrl);
            return false;
        }
    }

    public bool IsValidImageFile(string fileName, string contentType, long fileSize)
    {
        if (string.IsNullOrEmpty(fileName) || fileSize == 0)
            return false;
            
        if (fileSize > _maxFileSize)
            return false;
            
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return _allowedExtensions.Contains(extension);
    }
}
