namespace TheLightStore.Application.Interfaces;

/// <summary>
/// Abstraction cho file storage để Application layer không phụ thuộc vào ASP.NET Core
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Lấy đường dẫn wwwroot của ứng dụng
    /// </summary>
    string GetWebRootPath();
}
