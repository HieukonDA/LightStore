using Microsoft.Extensions.Options;
using TheLightStore.Application.Interfaces;
using TheLightStore.Infrastructure.Configuration;

namespace TheLightStore.Infrastructure.ExternalServices;

/// <summary>
/// Implementation của IFileStorageService không phụ thuộc vào ASP.NET Core
/// </summary>
public class FileStorageService : IFileStorageService
{
    private readonly string _webRootPath;

    public FileStorageService(IOptions<FileStorageOptions> options)
    {
        var webRoot = options.Value.WebRootPath;
        
        // Nếu là relative path, combine với current directory
        _webRootPath = Path.IsPathRooted(webRoot) 
            ? webRoot 
            : Path.Combine(Directory.GetCurrentDirectory(), webRoot);
    }

    public string GetWebRootPath()
    {
        return _webRootPath;
    }
}
