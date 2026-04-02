using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TheLightStore.Application.Exceptions;
using TheLightStore.Application.Helpers;
using TheLightStore.Application.Interfaces;
using TheLightStore.Application.Interfaces.Services;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Entities.Customers;
using static TheLightStore.Application.Dtos.AuthDto;
using static TheLightStore.Domain.Constants.Enum;

namespace TheLightStore.Application.Services.SysServices;

public class AuthService : IAuthService
{
    private readonly IIdentityService _identityService;
    private readonly IConfigurationService _configurationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailSenderService _emailSender;
    private readonly ICacheService _cacheService;
    private readonly ICodeService _codeService;
    private readonly IAuthDataService _authDataService;

    public AuthService(
        IIdentityService identityService,
        IConfigurationService configurationService,
        ICurrentUserService currentUserService,
        IEmailSenderService emailSender,
        ICacheService cacheService,
        ICodeService codeService,
        IAuthDataService authDataService)
    {
        _identityService = identityService;
        _configurationService = configurationService;
        _currentUserService = currentUserService;
        _emailSender = emailSender;
        _cacheService = cacheService;
        _codeService = codeService;
        _authDataService = authDataService;
    }

    private string GlobalUserId => _currentUserService.GetCurrentUserId() ?? string.Empty;

    public static ClaimsPrincipal? ValidateRefreshToken(IConfigurationService configService, string refreshToken)
    {
        var refreshSecret = configService.GetJwtRefreshKey() ?? "e8DsXj00N2D6w47+8YMYKQZMKfiL2poOzDAB9OxHUZw=";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(refreshSecret));
        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var principal = tokenHandler.ValidateToken(refreshToken, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configService.GetJwtIssuer(),
                ValidAudience = configService.GetJwtAudience(),
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var type = principal.FindFirst("type")?.Value;
            if (type != "refresh") return null;

            return principal;
        }
        catch
        {
            return null;
        }
    }

    public static bool IsValidUserIdentifier(string? input)
    {
        return InputValidationHelper.IsEmailFormat(input) || InputValidationHelper.IsPhoneFormat(input);
    }

    public async Task<SuccessResponseResult> Login(LoginDto model)
    {
        var result = new LoginResponse();
        var username = model.UserName?.Trim();
        if (string.IsNullOrWhiteSpace(username))
            throw new BadRequestException("Username is required.");
        if (!IsValidUserIdentifier(username))
            throw new BadRequestException("Invalid login format. Enter email or phone.");

        var user = await _identityService.FindByNameAsync(username!);

        if (user == null || !await _identityService.CheckPasswordAsync(user.UserId!, model.Password))
        {
            throw new UnauthorizedException("Invalid email or password.");
        }
        if (user.IsActive == false)
        {
            throw new ForbiddenException("Your account is inactive. Please contact support.");
        }

        var email = user?.Email ?? string.Empty;

        var role = await _identityService.GetUserRoleAsync(user!.UserId!);

        result.AccessToken = JwtHelper.GenerateAccessToken(_configurationService, user.UserId!, email, user.UserName, role!);
        result.RefreshToken = JwtHelper.GenerateRefreshToken(_configurationService, user.UserId!, email);
        result.User = user;
        return new SuccessResponseResult(result, "Login successful");
    }

    public async Task<ResponseResult> Me()
    {
        try
        {
            var userId = GlobalUserId;
            
            // Debug: Log the userId being fetched
            if (string.IsNullOrEmpty(userId))
                throw new NotFoundException("User ID not found in token. Please login again.");
            
            var model = await _identityService.FindByIdAsync(userId);
            if (model == null) throw new NotFoundException($"User not found with ID: {userId}");
            var employee = await _authDataService.GetEmployeeByUserIdAsync(userId);
            if(employee != null)
                model.EmployeeCode = employee.Code;
            return new SuccessResponseResult(model, "Get user data successfully");
        }
        catch (Exception ex)
        {
            return new ErrorResponseResult(MakeExceptionMessage(ex));
        }
    }

    private UserIdentifierType GetIdentifierType(string identifier)
    {
        if (InputValidationHelper.IsEmailFormat(identifier))
            return UserIdentifierType.Email;

        if (InputValidationHelper.IsPhoneFormat(identifier))
            return UserIdentifierType.Phone;

        throw new BadRequestException("Invalid username format. Must be email or phone.");
    }

    private async Task<UserIdentifierType> ValidateUserIdentifier(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new BadRequestException("Username is required.");
        var type = GetIdentifierType(identifier);

        bool exists = await _identityService.UserExistsAsync(identifier);

        if (exists)
            throw new BadRequestException(type == UserIdentifierType.Email
                ? "Email already exists."
                : "Phone number already exists.");

        return type;
    }

    public async Task<SuccessResponseResult> Register(RegisterDto model)
    {
        await _authDataService.BeginTransactionAsync();
        try
        {
            var username = model.UserName?.Trim();

            var type = await ValidateUserIdentifier(username!);
            var role = await _identityService.GetRoleByNameAsync("Customer");

            if (role is null)
                throw new BadRequestException("Role does not exist");

            var email = type == UserIdentifierType.Email ? username : $"{username}@noemail.local";
            var phoneNumber = type == UserIdentifierType.Phone ? username : null;

            var (success, errors) = await _identityService.CreateAsync(username!, model.Password, email, phoneNumber);
            if (!success)
            {
                var message = string.Join(", ", errors ?? new[] { "User creation failed" });
                throw new BadRequestException(message);
            }

            var user = await _identityService.FindByNameAsync(username!);
            if (user == null)
                throw new BadRequestException("Failed to retrieve created user");

            var userVm = user;
            userVm.RoleType = "Customer";
            
            string? confirmEmailLink = null;
            if (type == UserIdentifierType.Email)
            {
                var otpCode = _codeService.GenerateOtpCode();
                _cacheService.Set(username!, "OtpCode", otpCode, _configurationService.GetOtpExpiryMinutes());
                var sendMailResult = await SendEmailOTP(username!, otpCode);
                if (!sendMailResult.IsSuccess)
                {
                    await _authDataService.RollbackTransactionAsync();
                    throw new BadRequestException(sendMailResult.Message!);
                }
                confirmEmailLink = _configurationService.GetConfirmEmailUrl();
            }

            var CustomerType = await _authDataService.GetCustomerTypeByNameAsync("Khách Hàng Phổ Thông");

            if (CustomerType is null)
                throw new BadRequestException("CustomerType does not exist");
            
            var newCustomer = new Customer
            {
                Code = await _codeService.GenerateCustomerCodeAsync(),
                IdentityUserId = user.UserId,
                CustomerTypeId = CustomerType.Id,
                TotalCancelOrder = 0,
                TotalCompletedOrder = 0,
                TotalBuyingProduct = 0,
                TotalPoints = 0,
                TotalPayment = 0,
                IsActive = true
            };
            await _authDataService.AddCustomerAsync(newCustomer);
            await _authDataService.SaveChangesAsync();
            await _authDataService.CommitTransactionAsync();

            var responseData = new
            {
                User = userVm,
                ConfirmEmailLink = confirmEmailLink
            };
            return new SuccessResponseResult(responseData, "Registration successful");
        }
        catch (BadRequestException)
        {
            await _authDataService.RollbackTransactionAsync();
            throw;
        }
        catch (Exception)
        {
            await _authDataService.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<SuccessResponseResult> ConfirmEmail(string otp, string email)
    {
        var user = await _identityService.FindByEmailAsync(email);
        if (user == null)
        {
            throw new UnauthorizedException("Email not found");
        }
        var storedOtp = _cacheService.Get(email, "OtpCode");
        if (string.IsNullOrEmpty(storedOtp))
        {
            throw new BadRequestException("Invalid request information");
        }
        if (storedOtp != otp)
        {
            throw new BadRequestException("Invalid OTP");
        }
        
        user.IsActive = true;
        var (success, errors) = await _identityService.UpdateAsync(user);
        if (!success)
        {
            throw new BadRequestException(string.Join(", ", errors ?? new[] { "Failed to update user" }));
        }
        
        _cacheService.Remove(email, "OtpCode");

        var userVm = user;
        var roleName = await _identityService.GetUserRoleAsync(user.UserId!);
        userVm.RoleType = roleName;
        
        return new SuccessResponseResult(userVm, "Email confirmation successful");
    }

    public async Task<ResponseResult> ChangePassword(ChangePasswordDto model)
    {
        var user = await _identityService.FindByIdAsync(GlobalUserId);
        if (user == null)
            return new ErrorResponseResult(message: "User Not Found");
        if (!model.NewPassword.Equals(model.ConfirmPassword))
            return new ErrorResponseResult(message: "Passwords do not match");
        
        var (success, errors) = await _identityService.ChangePasswordAsync(GlobalUserId, model.CurrentPassword, model.NewPassword);
        if (!success)
            return new ErrorResponseResult(message: string.Join(", ", errors ?? new[] { "Password change failed" }));
        
        return new SuccessResponseResult(message: "Password changed successfully");
    }

    public async Task<ResponseResult> ForgotPassword(ForgotPasswordDto model)
    {
        var user = await _identityService.FindByEmailAsync(model.Email);
        object data = new { };
        if (user != null)
        {
            var otpCode = _codeService.GenerateOtpCode();
            _cacheService.Set(user.Email!, "OtpCode", otpCode, _configurationService.GetOtpExpiryMinutes());
            var sendMailResult = await SendEmailOTP(user.Email!, otpCode);
            if (!sendMailResult.IsSuccess)
                throw new BadRequestException(sendMailResult.Message!);
            data = new { Email = model.Email };
        }
        return new ResponseResult
        {
            IsSuccess = true,
            Message = "If an account exists, a reset link has been sent.",
            Data = data
        };
    }

    public SuccessResponseResult VerifyOTP(VerifyOtpDto model)
    {
        var cachedOtp = _cacheService.Get(model.Email, "OtpCode");
        if (cachedOtp is null)
            throw new BadRequestException("OTP expired or not found");

        if (cachedOtp != model.Otp)
            throw new BadRequestException("Invalid OTP");

        var sessionKey = Guid.NewGuid().ToString();
        _cacheService.Set(sessionKey, "ResetSessionKey", model.Email, 10);
        return new SuccessResponseResult
        {
            IsSuccess = true,
            Message = "OTP verified successfully",
            Data = new { SessionKey = sessionKey }
        };
    }

    public async Task<ResponseResult> ResetPassword(ResetPasswordDto model)
    {
        var cachedEmail = _cacheService.Get(model.SessionKey, "ResetSessionKey");
        if (cachedEmail is null)
            throw new BadRequestException("Session expired or invalid");
        var email = cachedEmail.ToString();

        var user = await _identityService.FindByEmailAsync(email);
        if (user is null)
            throw new NotFoundException("User not found");

        if (model.NewPassword != model.ConfirmPassword)
            throw new BadRequestException("Passwords do not match");

        var token = await _identityService.GeneratePasswordResetTokenAsync(user.UserId!);
        var (success, errors) = await _identityService.ResetPasswordAsync(user.UserId!, token, model.NewPassword);

        if (!success)
            throw new BadRequestException(string.Join(", ", errors ?? new[] { "Password reset failed" }));

        _cacheService.Remove(model.SessionKey, "ResetSessionKey");

        return new ResponseResult
        {
            IsSuccess = true,
            Message = "Password has been reset successfully",
            Data = new { Email = email }
        };
    }

    public async Task<RegisterResponse> UpdateProfile(UpdateProfileDto model)
    {
        var response = new RegisterResponse();
        var userId = GlobalUserId;
        if (string.IsNullOrEmpty(userId))
        {
            response.Message = "User not authenticated";
            response.IsSuccess = false;
            return response;
        }
        var user = await _identityService.FindByIdAsync(userId);
        if (user == null)
        {
            response.Message = "Email not found";
            response.IsSuccess = false;
            return response;
        }
        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Address = model.Address;
        var (success, errors) = await _identityService.UpdateAsync(user);
        if (!success)
        {
            response.Message = "Profile update failed";
            response.IsSuccess = false;
            return response;
        }
        response.User = user;
        response.Message = "Profile updated successfully";
        response.IsSuccess = true;
        return response;
    }

    public async Task<RegisterResponse> ResendOTPRegister(string email)
    {
        var response = new RegisterResponse();
        var user = await _identityService.FindByEmailAsync(email);
        if (user == null)
        {
            response.Message = "Email not found";
            response.IsSuccess = false;
            return response;
        }
        var otpCode = _codeService.GenerateOtpCode();
        _cacheService.Set(email, "OtpCode", otpCode, _configurationService.GetOtpExpiryMinutes());
        var sendMailResult = await SendEmailOTP(email, otpCode);
        if (!sendMailResult.IsSuccess)
        {
            response.Message = sendMailResult.Message;
            response.IsSuccess = false;
            return response;
        }
        response.Message = "Resend OTP successfully";
        response.IsSuccess = true;
        return response;
    }

    public async Task<SuccessResponseResult> RefreshToken(string refreshToken)
    {
        var principal = ValidateRefreshToken(_configurationService, refreshToken);
        
        if (principal == null)
            throw new UnauthorizedException("Invalid or expired refresh token");

        var userId = principal.FindFirst("UserId")?.Value;
        var email = principal.FindFirst(ClaimTypes.Email)?.Value;
        var userName = principal.FindFirst("UserName")?.Value;
        
        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedException("Invalid token payload");

        var newAccessToken = JwtHelper.GenerateAccessToken(_configurationService, userId, email ?? string.Empty, userName);
        var newRefreshToken = JwtHelper.GenerateRefreshToken(_configurationService, userId, email ?? string.Empty);

        return new SuccessResponseResult(new { accessToken = newAccessToken, refreshToken = newRefreshToken }, "Token refreshed successfully");
    }

    private async Task<ResponseResult> SendEmailOTP(string email, string otpCode)
    {
        var subject = "Your OTP Code";
        var confirmEmailUrl =
            _configurationService.GetConfirmEmailUrl() +
            $"?Email={Uri.EscapeDataString(email)}" +
            $"&Otp={Uri.EscapeDataString(otpCode)}";
        var body = $@"Your OTP code is {otpCode}. Please don't share it with anyone!
            Confirm your email by clicking the link below:
            {confirmEmailUrl}
            ";
        var sendMailResult = await _emailSender.SendMailAsync(
            fromEmail: "dev.free.789@gmail.com",
            fromPassWord: "xonb lovh nllk fxel",
            toEmail: email,
            sendMailTitle: subject,
            sendMailBody: body
        );
        return sendMailResult;
    }

    private string MakeExceptionMessage(Exception ex)
    {
        return $"Error: {ex.Message}";
    }
}
