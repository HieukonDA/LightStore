using Microsoft.AspNetCore.Mvc;

namespace TheLightStore.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    readonly private IAuthService _authService;
    readonly private ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var result = await _authService.LoginAsync(loginDto);
        if (result.Success)
        {
            return Ok(result.Data);
        }
        return BadRequest(result.Errors);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        var result = await _authService.RegisterAsync(registerDto);
        if (result.Success)
        {
            return Ok(result.Data);
        }
        return BadRequest(result.Errors);
    }
}