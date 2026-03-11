namespace TheLightStore.Application.DTOs.Images;

/// <summary>
/// DTO để upload hình ảnh từ Web layer sang Application layer
/// Thay thế IFormFile để tránh phụ thuộc vào ASP.NET Core
/// </summary>
public class ImageUploadDto
{
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public required Stream FileStream { get; set; }
    public long FileSize { get; set; }
}
