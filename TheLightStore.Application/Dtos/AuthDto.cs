using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using TheLightStore.Domain.Commons.Ultilities;

namespace TheLightStore.Application.Dtos;

public class AuthDto
{
    public class LoginDto
    {
        [Required]
        public string? UserName { get; set; }

        [Required]
        public string Password { get; set; } = "123456y";
    }

    public class LoginResponse
    {
        public string? AccessToken { get; set; }
        public string? Message { get; set; }
        public bool? IsSuccess { get; set; }
        public MeDto? User { get; set; }
        public string? RefreshToken { get; set; }

    }
    public class LoginResponseSlim
    {
        public string? AccessToken { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? RefreshToken { get; set; }
    }
    public class RegisterDto
    {
        [Required]
        public string? UserName { get; set; }
        [Required]
        public string Password { get; set; } = "123456y";
    }

    public class RegisterResponse
    {
        public string? Message { get; set; }
        public bool? IsSuccess { get; set; }
        public MeDto? User { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string? ConfirmEmailLink { get; set; }
    }

    public class ConfirmEmailDto
    {
        public required string Otp { get; set; }
        public required string Email { get; set; }
    }

    public class MeDto
    {
        public string? UserId { get; set; }
        public string? EmployeeCode { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Sex { get; set; }
        public string? Birthday { get; set; }
        public string? Address { get; set; }
        public string? Note { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }
        public bool? IsActive { get; set; }
        public string? RoleType { get; set; }
    }

    public class ChangePasswordDto
    {
        public required string CurrentPassword { get; set; }
        public required string NewPassword { get; set; }
        public required string ConfirmPassword { get; set; }
    }

    public class ChangePasswordResponse
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
    }
    public class ForgotPasswordDto
    {
        [Required]
        public required string Email { get; set; }
    }
    public class VerifyOtpDto
    {
        [Required]
        public required string Email { get; set; }

        [Required]
        public required string Otp { get; set; }
    }
    public class ResetPasswordDto
    {
        [Required]
        public required string SessionKey { get; set; }

        [Required]
        public required string NewPassword { get; set; }

        [Required]
        public required string ConfirmPassword { get; set; }
    }

    public class UpdateProfileDto
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Address { get; set; }
        public bool? Sex { get; set; }

        [JsonConverter(typeof(DateOnlyJsonConverter))]
        public DateOnly? Birthday { get; set; }
    }

    public class OauthDto
    {
        public required string UserId { get; set; }
        public required string ProviderName { get; set; }
        public required string Email { get; set; }
        public required string FullName { get; set; }
    }

    public class OauthRespone : LoginResponse
    {
        public string? ProviderUserName { get; set; }
        public string? ProviderUserEmail { get; set; }
    }

    public class ResendOTPDto
    {
        public required string Email { get; set; }
    }
}
