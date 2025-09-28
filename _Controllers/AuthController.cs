

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
        var result = await _authService.SendRegistrationOtpAsync(registerDto);
        if (result.Success)
        {
            return Ok(new
            {
                message = "Registration OTP sent successfully. Please check your email for the OTP to verify your account.",
                success = true
            });
        }
        return BadRequest(new
        {
            message = "Registration failed",
            errors = result.Errors
        });
    }

    [HttpPost("verify-registration-otp")]
    public async Task<IActionResult> VerifyRegistrationOtp([FromBody] VerifyRegistrationOtpDto dto)
    {
        var result = await _authService.VerifyRegistrationOtpAsync(dto);
        if (result.Success)
        {
            return Ok(new
            {
                message = "Account verified successfully. You are now logged in.",
                data = result.Data,
                success = true
            });
        }

        return BadRequest(new
        {
            message = "Verification failed",
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

    [Authorize]
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto updateProfileDto)
    {
        var result = await _authService.UpdateProfileAsync(updateProfileDto);
        
        if (result.Success)
        {
            return Ok(new
            {
                message = result.Message,
                data = result.Data,
                success = true
            });
        }

        return BadRequest(new
        {
            message = result.Message,
            errors = result.Errors
        });
    }

    #region Admin - Customer Management

    [Authorize(Roles = "Admin")]
    [HttpGet("roles")]
    public async Task<IActionResult> GetAllRoles()
    {
        var result = await _authService.GetAllRolesAsync();
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("customers/{customerId}/roles")]
    public async Task<IActionResult> GetCustomerRoles(int customerId)
    {
        var result = await _authService.GetCustomerRolesDetailAsync(customerId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("customers")]
    public async Task<IActionResult> GetCustomers([FromQuery] PagedRequest request)
    {
        var result = await _authService.GetCustomersAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("customers/{customerId}")]
    public async Task<IActionResult> GetCustomerById(int customerId)
    {
        var result = await _authService.GetCustomerByIdAsync(customerId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("customers/{customerId}")]
    public async Task<IActionResult> DeleteCustomer(int customerId)
    {
        var result = await _authService.DeleteCustomerAsync(customerId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("customers/{customerId}/role")]
    public async Task<IActionResult> UpdateCustomerRole(int customerId, [FromBody] UpdateCustomerRoleDto updateRoleDto)
    {
        var result = await _authService.UpdateCustomerRoleAsync(customerId, updateRoleDto);
        
        if (result.Success)
        {
            return Ok(new
            {
                message = result.Message,
                success = true
            });
        }

        return BadRequest(new
        {
            message = result.Message,
            errors = result.Errors
        });
    }

    #endregion

}