

using Microsoft.AspNetCore.Authorization;

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
            return Ok(new
            {
                message = "Login successful",
                data = result.Data
            });
        }
        return BadRequest(new
        {
            message = "Login failed",
            errors = result.Errors
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        var result = await _authService.RegisterAsync(registerDto);
        if (result.Success)
        {
            return Ok(new
            {
                message = "Registration successful",
                data = result.Data
            });
        }
        return BadRequest(new
        {
            message = "Registration failed",
            errors = result.Errors
        });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
    {
        var result = await _authService.ForgotPasswordAsync(forgotPasswordDto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [Authorize] // chỉ user đã login mới đổi pass được
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        var result = await _authService.ChangePasswordAsync(changePasswordDto);

        if (result.Success)
        {
            return Ok(new
            {
                message = result.Message,
                success = true
            });
        }

        _logger.LogWarning("Change password failed. Errors: {Errors}", string.Join(", ", result.Errors));
        return BadRequest(new
        {
            message = result.Message,
            errors = result.Errors
        });
    }

    [HttpPost("reset-password-otp")]
    public async Task<IActionResult> ResetPasswordWithOtp([FromBody] ResetPasswordDto dto)
    {
        var result = await _authService.ResetPasswordWithOtpAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("resend-otp")]
    public async Task<IActionResult> ResendOtp([FromBody] string email)
    {
        var result = await _authService.ResendOtpAsync(email);
        return result.Success ? Ok(result) : BadRequest(result);
    }

}