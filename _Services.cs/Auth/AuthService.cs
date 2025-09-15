


namespace TheLightStore.Services;

public class AuthService : IAuthService
{
    private readonly DBContext _context;
    private readonly IUserRepo _userRepo;
    private readonly ILogger<AuthService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IEmailService _emailService;
    private readonly IRbacService _rbacService;
    private static readonly Dictionary<string, OtpData> _otpCache = new();
    private static readonly object _cacheLock = new();

    public AuthService(DBContext context, IUserRepo userRepo, ILogger<AuthService> logger, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IEmailService emailService, IRbacService rbacService)
    {
        _context = context;
        _userRepo = userRepo;
        _logger = logger;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
        _emailService = emailService;
        _rbacService = rbacService;
    }

    #region implement interfaces

    public async Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDto loginDto)
    {
        try
        {
            _logger.LogInformation("Attempting login for user: {Email}", loginDto.Email);
            //validate loginDto
            if (string.IsNullOrWhiteSpace(loginDto.Email) || string.IsNullOrWhiteSpace(loginDto.Password))
            {
                return ServiceResult<AuthResponseDto>.FailureResult("Email and password are required.", new List<string>());
            }

            //find user by email
            var user = await _userRepo.GetUserByEmailAsync(loginDto.Email);

            if (user == null)
            {
                _logger.LogWarning("Login failed for user: {Email}. User not found.", loginDto.Email);
                return ServiceResult<AuthResponseDto>.FailureResult("Invalid email or password.", new List<string>());
            }

            // verify password
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                _logger.LogWarning("Login failed for user: {Email}. Invalid password.", loginDto.Email);
                return ServiceResult<AuthResponseDto>.FailureResult("Invalid email or password.", new List<string>());
            }

            //create token
            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();

            //save refresh token to database
            await SaveRefreshTokenAsync(user.Id, refreshToken);

            var response = new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpireAt = DateTime.Now.AddMinutes(GetJwtExpirationMinutes()),
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Phone = user.Phone,
                    UserType = user.UserType,
                    CreatedAt = user.CreatedAt,
                    Roles = await _rbacService.GetUserRolesAsync(user.Id)
                }
            };

            _logger.LogInformation($"Login successful for user: {user.Email}");

            return ServiceResult<AuthResponseDto>.SuccessResult(response, "Login successful");


        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, $"Login failed with: {loginDto.Email}");
            return ServiceResult<AuthResponseDto>.FailureResult("An error occurred while logging in.", new List<string> { ex.Message });
        }

    }

    public async Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterDto registerDto)
    {
        try
        {
            _logger.LogInformation("Registering a new user with email: {Email}", registerDto.Email);
            //validate registerDto
            var errors = ValidateRegisterDto(registerDto);
            if (errors.Any())
            {
                return ServiceResult<AuthResponseDto>.FailureResult("Validation errors occurred.", errors);
            }

            //check user exist
            var existingUser = await _userRepo.GetUserByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Registration failed. Email {Email} is already in use.", registerDto.Email);
                return ServiceResult<AuthResponseDto>.FailureResult("Email is already in use.", new List<string>());
            }

            //hash password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            //create new user
            var newUser = new User
            {
                Email = registerDto.Email,
                PasswordHash = hashedPassword,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Phone = registerDto.Phone,
                UserType = "customer",
                CreatedAt = DateTime.Now
            };

            //add user
            await _userRepo.AddUserAsync(newUser);
            await _context.SaveChangesAsync();

            //send welcome email
            await _emailService.SendEmailAsync(
                newUser.Email,
                "Welcome to LightStore!",
                $"<h2>Hello {newUser.FirstName}!</h2><p>Thank you for registering at LightStore.</p>"
            );

            //create token
            var accessToken = GenerateAccessToken(newUser);
            var refreshToken = GenerateRefreshToken();

            //save refresh token
            await SaveRefreshTokenAsync(newUser.Id, refreshToken);

            //return response
            var response = new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpireAt = DateTime.Now.AddMinutes(GetJwtExpirationMinutes()),
                User = new UserDto
                {
                    Id = newUser.Id,
                    Email = newUser.Email,
                    FirstName = newUser.FirstName,
                    LastName = newUser.LastName,
                    Phone = newUser.Phone,
                    UserType = newUser.UserType,
                    CreatedAt = newUser.CreatedAt
                }
            };

            _logger.LogInformation("User registered successfully with email: {Email}", registerDto.Email);
            return ServiceResult<AuthResponseDto>.SuccessResult(response);

        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "An error occurred while registering a new user.");
            return ServiceResult<AuthResponseDto>.FailureResult("An error occurred while processing your request.", new List<string>());
        }
    }

    public async Task<ServiceResult<bool>> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(forgotPasswordDto.Email))
            {
                return ServiceResult<bool>.FailureResult("Email is required.", new List<string>());
            }

            var user = await _userRepo.GetUserByEmailAsync(forgotPasswordDto.Email);
            if (user == null)
            {
                _logger.LogWarning("Forgot password attempted for non-existing email: {Email}", forgotPasswordDto.Email);
                // Vẫn trả về success để không lộ thông tin user
                return ServiceResult<bool>.SuccessResult(true, "If the email exists, you will receive an OTP code.");
            }

            // Tạo OTP 6 số
            var otp = GenerateOTP();
            var expiryTime = DateTime.Now.AddMinutes(10); // OTP hết hạn sau 10 phút

            // Lưu OTP vào cache
            lock (_cacheLock)
            {
                _otpCache[forgotPasswordDto.Email] = new OtpData
                {
                    Email = forgotPasswordDto.Email,
                    Otp = otp,
                    ExpiresAt = expiryTime,
                    Attempts = 0
                };
            }

            // Gửi email OTP
            await _emailService.SendEmailAsync(
                user.Email,
                "Password Reset OTP",
                $"<h2>Password Reset Request</h2>" +
                $"<p>Your OTP code is: <strong>{otp}</strong></p>" +
                $"<p>This code will expire in 10 minutes.</p>" +
                $"<p>If you didn't request this, please ignore this email.</p>"
            );

            _logger.LogInformation("OTP sent for password reset to email: {Email}", forgotPasswordDto.Email);

            return ServiceResult<bool>.SuccessResult(true, "OTP has been sent to your email address.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Forgot password OTP process failed for {Email}", forgotPasswordDto.Email);
            return ServiceResult<bool>.FailureResult("An error occurred while processing your request.", new List<string> { ex.Message });
        }

    }

    public async Task<ServiceResult<bool>> ChangePasswordAsync(ChangePasswordDto changePasswordDto)
    {
        try
        {
            // Lấy userId từ Claims
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return ServiceResult<bool>.FailureResult("Unauthorized.", new List<string>());
            }

            var user = await _userRepo.GetUserByIdAsync(userId);
            if (user == null)
            {
                return ServiceResult<bool>.FailureResult("User not found.", new List<string>());
            }

            // check curerent password
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.PasswordHash);
            if (!isPasswordValid)
            {
                return ServiceResult<bool>.FailureResult("Current password is incorrect.", new List<string>());
            }

            // validate new password
            if (!IsValidPassword(changePasswordDto.NewPassword))
            {
                return ServiceResult<bool>.FailureResult(
                    "Password must be at least 12 characters, include upper/lowercase letters, numbers and special characters.",
                    new List<string>());
            }

            // Hash mật khẩu mới
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
            var updateResult = await _userRepo.UpdateUserAsync(user);

            if (!updateResult)
            {
                return ServiceResult<bool>.FailureResult("Failed to update password.", new List<string>());
            }

            _logger.LogInformation("Password changed successfully for user {Email}", user.Email);
            return ServiceResult<bool>.SuccessResult(true, "Password changed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Change password failed.");
            return ServiceResult<bool>.FailureResult("An error occurred while changing password.", new List<string> { ex.Message });
        }

    }

    public async Task<ServiceResult<bool>> ResetPasswordWithOtpAsync(ResetPasswordDto resetDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(resetDto.Email) ||
                string.IsNullOrWhiteSpace(resetDto.Otp) ||
                string.IsNullOrWhiteSpace(resetDto.NewPassword))
            {
                return ServiceResult<bool>.FailureResult("Email, OTP, and new password are required.", new List<string>());
            }

            OtpData otpData;
            lock (_cacheLock)
            {
                if (!_otpCache.TryGetValue(resetDto.Email, out otpData))
                {
                    return ServiceResult<bool>.FailureResult("Invalid or expired OTP.", new List<string>());
                }
            }

            // Kiểm tra số lần thử
            if (otpData.Attempts >= 5)
            {
                lock (_cacheLock)
                {
                    _otpCache.Remove(resetDto.Email);
                }
                return ServiceResult<bool>.FailureResult("Too many failed attempts. Please request a new OTP.", new List<string>());
            }

            // Kiểm tra OTP hết hạn
            if (DateTime.Now > otpData.ExpiresAt)
            {
                lock (_cacheLock)
                {
                    _otpCache.Remove(resetDto.Email);
                }
                return ServiceResult<bool>.FailureResult("OTP has expired. Please request a new one.", new List<string>());
            }

            // Kiểm tra OTP đúng
            if (otpData.Otp != resetDto.Otp)
            {
                lock (_cacheLock)
                {
                    otpData.Attempts++;
                }
                return ServiceResult<bool>.FailureResult($"Invalid OTP. {5 - otpData.Attempts} attempts remaining.", new List<string>());
            }

            // Tìm user
            var user = await _userRepo.GetUserByEmailAsync(resetDto.Email);
            if (user == null)
            {
                return ServiceResult<bool>.FailureResult("User not found.", new List<string>());
            }

            // Validate password mới
            if (!IsValidPassword(resetDto.NewPassword))
            {
                return ServiceResult<bool>.FailureResult(
                    "Password must be at least 8 characters, include upper/lowercase letters, numbers and special characters.",
                    new List<string>());
            }

            // Hash mật khẩu mới
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(resetDto.NewPassword);

            await _userRepo.UpdateUserAsync(user);
            await _context.SaveChangesAsync();

            // Xóa OTP khỏi cache
            lock (_cacheLock)
            {
                _otpCache.Remove(resetDto.Email);
            }

            _logger.LogInformation("Password reset successfully with OTP for user {Email}", user.Email);
            return ServiceResult<bool>.SuccessResult(true, "Password reset successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Reset password with OTP failed.");
            return ServiceResult<bool>.FailureResult("An error occurred while resetting password.", new List<string> { ex.Message });
        }
    }
    

    public async Task<ServiceResult<bool>> ResendOtpAsync(string email)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return ServiceResult<bool>.FailureResult("Email is required.", new List<string>());
            }

            var user = await _userRepo.GetUserByEmailAsync(email);
            if (user == null)
            {
                return ServiceResult<bool>.SuccessResult(true, "If the email exists, a new OTP will be sent.");
            }

            // Kiểm tra xem có OTP cũ không
            lock (_cacheLock)
            {
                if (_otpCache.ContainsKey(email))
                {
                    var existingOtp = _otpCache[email];
                    // Chỉ cho phép gửi lại nếu OTP cũ sắp hết hạn (còn < 2 phút)
                    if (DateTime.Now < existingOtp.ExpiresAt.AddMinutes(-8))
                    {
                        return ServiceResult<bool>.FailureResult("Please wait before requesting a new OTP.", new List<string>());
                    }
                }
            }

            // Tạo OTP mới
            var newOtp = GenerateOTP();
            var expiryTime = DateTime.Now.AddMinutes(10);

            lock (_cacheLock)
            {
                _otpCache[email] = new OtpData
                {
                    Email = email,
                    Otp = newOtp,
                    ExpiresAt = expiryTime,
                    Attempts = 0
                };
            }

            // Gửi email OTP mới
            await _emailService.SendEmailAsync(
                user.Email,
                "New Password Reset OTP",
                $"<h2>New Password Reset OTP</h2>" +
                $"<p>Your new OTP code is: <strong>{newOtp}</strong></p>" +
                $"<p>This code will expire in 10 minutes.</p>"
            );

            _logger.LogInformation("New OTP sent for password reset to email: {Email}", email);
            return ServiceResult<bool>.SuccessResult(true, "A new OTP has been sent to your email.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Resend OTP failed for {Email}", email);
            return ServiceResult<bool>.FailureResult("An error occurred while sending OTP.", new List<string> { ex.Message });
        }
    }




    #endregion

    #region helper methods

    private List<string> ValidateRegisterDto(RegisterDto registerDto)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(registerDto.Email))
        {
            errors.Add("Email is required.");
        }

        if (!IsValidEmail(registerDto.Email))
        {
            errors.Add("Invalid email format.");
        }

        if (!IsValidPassword(registerDto.Password))
        {
            errors.Add("Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one number, and one special character.");
        }

        return errors;
    }

    private bool IsValidEmail(string Email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(Email);
            return addr.Address == Email;
        }
        catch (System.Exception)
        {
            return false;
        }
    }

    private bool IsValidPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            return false;

        bool hasUpperCase = password.Any(char.IsUpper);
        bool hasLowerCase = password.Any(char.IsLower);
        bool hasNumber = password.Any(char.IsDigit);
        bool hasSpecialChar = password.Any(ch => !char.IsLetterOrDigit(ch));

        return hasUpperCase && hasLowerCase && hasNumber && hasSpecialChar;
    }


    // token ================================================================================================

    public string GenerateAccessToken(User user)
    {
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not found"));
        var tokenHandler = new JwtSecurityTokenHandler();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("UserType", user.UserType)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddMinutes(GetJwtExpirationMinutes()),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"]
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private async Task SaveRefreshTokenAsync(int userId, string refreshToken)
    {
        var user = await _userRepo.GetUserByIdAsync(userId);
        if (user == null) return;

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7); // ví dụ refresh token sống 7 ngày

        await _userRepo.UpdateUserAsync(user);
        await _context.SaveChangesAsync();
    }

    private int GetJwtExpirationMinutes()
    {
        return int.TryParse(_configuration["Jwt:ExpirationMinutes"], out var minutes) ? minutes : 60;
    }

    

    // 6. Helper method để tạo OTP
    private string GenerateOTP()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString(); // Tạo số 6 chữ số
    }
    

    #endregion
}