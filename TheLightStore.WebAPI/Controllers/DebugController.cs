using Microsoft.AspNetCore.Mvc;
using TheLightStore.Application.Interfaces;

namespace TheLightStore.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class DebugController : ControllerBase
{
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<DebugController> _logger;

    public DebugController(IEncryptionService encryptionService, ILogger<DebugController> logger)
    {
        _encryptionService = encryptionService;
        _logger = logger;
    }

    [HttpPost("test-encryption")]
    public IActionResult TestEncryption([FromBody] string testData)
    {
        try
        {
            _logger.LogInformation("Testing encryption with data: {Data}", testData);
            
            var encrypted = _encryptionService.Encrypt(testData);
            _logger.LogInformation("Encrypted result length: {Length}", encrypted.Length);
            
            var decrypted = _encryptionService.Decrypt(encrypted);
            _logger.LogInformation("Decrypted result: {Decrypted}", decrypted);
            
            return Ok(new 
            { 
                Original = testData,
                Encrypted = encrypted,
                EncryptedLength = encrypted.Length,
                Decrypted = decrypted,
                Success = testData == decrypted
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Encryption test failed");
            return BadRequest(new { Error = ex.Message });
        }
    }
}