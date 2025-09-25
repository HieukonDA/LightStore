


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
    private static readonly Dictionary<string, PendingRegistration> _pendingRegistrations = new();
  
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
        var user = await _userRepo.GetUserByEmailAsync(loginDto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
        {
            return ServiceResult<AuthResponseDto>.FailureResult("Invalid email or password.", new List<string>());
        }

        // Lấy roles từ RBAC service
        var roles = await _rbacService.GetUserRolesAsync(user.Id);

        // Tạo access token có chứa roles
        var accessToken = GenerateAccessToken(user, roles);

        var refreshToken = GenerateRefreshToken();

        await SaveRefreshTokenAsync(user.Id, refreshToken);

        var response = new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpireAt = DateTime.UtcNow.AddMinutes(GetJwtExpirationMinutes()),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.Phone,
                UserType = user.UserType,
                CreatedAt = user.CreatedAt,
                Roles = roles.ToList() // gán roles luôn cho DTO
            }
        };

        return ServiceResult<AuthResponseDto>.SuccessResult(response, "Login successful");
    }


    /// <summary>
    /// Bước 1: Gửi OTP để verify email trong quá trình đăng ký
    /// </summary>
    public async Task<ServiceResult<bool>> SendRegistrationOtpAsync(RegisterDto registerDto)
    {
        try
        {
            // Validate input
            var errors = ValidateRegisterDto(registerDto);
            if (errors.Any())
            {
                return ServiceResult<bool>.FailureResult("Validation errors occurred.", errors);
            }

            // Kiểm tra email đã tồn tại
            var existingUser = await _userRepo.GetUserByEmailAsync(registerDto.Email);
            if (existingUser != null)
            {
                return ServiceResult<bool>.FailureResult("Email already exists.", new List<string>());
            }

            // Tạo OTP
            var otp = GenerateOTP();
            var expiryTime = DateTime.UtcNow.AddMinutes(10);

            // Lưu thông tin đăng ký tạm thời
            lock (_cacheLock)
            {
                _pendingRegistrations[registerDto.Email] = new PendingRegistration
                {
                    Email = registerDto.Email,
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    Phone = registerDto.Phone,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                    Otp = otp,
                    ExpiresAt = expiryTime,
                    Attempts = 0
                };
            }

            // Gửi email OTP
            await _emailService.SendEmailAsync(
                registerDto.Email,
                "Email Verification - LightStore",
                $"<h2>Welcome to LightStore!</h2>" +
                $"<p>Thank you for registering with us. Please verify your email address.</p>" +
                $"<p>Your verification code is: <strong>{otp}</strong></p>" +
                $"<p>This code will expire in 10 minutes.</p>" +
                $"<p>If you didn't create an account, please ignore this email.</p>"
            );

            _logger.LogInformation("Registration OTP sent to email: {Email}", registerDto.Email);
            return ServiceResult<bool>.SuccessResult(true, "OTP has been sent to your email address. Please verify to complete registration.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send registration OTP for {Email}", registerDto.Email);
            return ServiceResult<bool>.FailureResult("An error occurred while sending verification email.", new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// Bước 2: Xác thực OTP và tạo tài khoản
    /// </summary>
    public async Task<ServiceResult<AuthResponseDto>> VerifyRegistrationOtpAsync(VerifyRegistrationOtpDto verifyDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(verifyDto.Email) || string.IsNullOrWhiteSpace(verifyDto.Otp))
            {
                return ServiceResult<AuthResponseDto>.FailureResult("Email and OTP are required.", new List<string>());
            }

            PendingRegistration pendingReg;
            lock (_cacheLock)
            {
                if (!_pendingRegistrations.TryGetValue(verifyDto.Email, out pendingReg))
                {
                    return ServiceResult<AuthResponseDto>.FailureResult("Invalid or expired verification code.", new List<string>());
                }
            }

            // Kiểm tra số lần thử
            if (pendingReg.Attempts >= 5)
            {
                lock (_cacheLock)
                {
                    _pendingRegistrations.Remove(verifyDto.Email);
                }
                return ServiceResult<AuthResponseDto>.FailureResult("Too many failed attempts. Please request a new verification code.", new List<string>());
            }

            // Kiểm tra hết hạn
            if (DateTime.UtcNow > pendingReg.ExpiresAt)
            {
                lock (_cacheLock)
                {
                    _pendingRegistrations.Remove(verifyDto.Email);
                }
                return ServiceResult<AuthResponseDto>.FailureResult("Verification code has expired. Please request a new one.", new List<string>());
            }

            // Kiểm tra OTP
            if (pendingReg.Otp != verifyDto.Otp)
            {
                lock (_cacheLock)
                {
                    pendingReg.Attempts++;
                }
                return ServiceResult<AuthResponseDto>.FailureResult($"Invalid verification code. {5 - pendingReg.Attempts} attempts remaining.", new List<string>());
            }

            // OTP đúng -> tạo user
            var user = new User
            {
                FirstName = pendingReg.FirstName,
                LastName = pendingReg.LastName,
                Email = pendingReg.Email,
                Phone = pendingReg.Phone,
                PasswordHash = pendingReg.PasswordHash,
                UserType = "customer",
                EmailVerified = true, // Đánh dấu đã verify email
                CreatedAt = DateTime.UtcNow
            };

            await _userRepo.AddUserAsync(user);
            await _context.SaveChangesAsync();

            // Gửi email chào mừng
            await _emailService.SendEmailAsync(
                user.Email,
                "Welcome to LightStore!",
                $"<h2>Hello {user.FirstName}!</h2>" +
                $"<p>Your account has been successfully created and verified.</p>" +
                $"<p>Thank you for joining LightStore!</p>"
            );

            // Gán role mặc định
            await _rbacService.AssignRoleToUserAsync(user.Id, 2); // roleId=2 là Customer

            var roles = await _rbacService.GetUserRolesAsync(user.Id);

            // Tạo token
            var accessToken = GenerateAccessToken(user, roles);
            var refreshToken = GenerateRefreshToken();

            await SaveRefreshTokenAsync(user.Id, refreshToken);

            // Xóa pending registration
            lock (_cacheLock)
            {
                _pendingRegistrations.Remove(verifyDto.Email);
            }

            var response = new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpireAt = DateTime.UtcNow.AddMinutes(GetJwtExpirationMinutes()),
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Phone = user.Phone,
                    UserType = user.UserType,
                    CreatedAt = user.CreatedAt,
                    Roles = roles.ToList()
                }
            };

            _logger.LogInformation("User registered successfully with email: {Email}", user.Email);
            return ServiceResult<AuthResponseDto>.SuccessResult(response, "Registration successful! Welcome to LightStore.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify registration OTP for {Email}", verifyDto.Email);
            return ServiceResult<AuthResponseDto>.FailureResult("An error occurred while verifying your email.", new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// Gửi lại OTP cho registration
    /// </summary>
    public async Task<ServiceResult<bool>> ResendRegistrationOtpAsync(string email)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return ServiceResult<bool>.FailureResult("Email is required.", new List<string>());
            }

            PendingRegistration pendingReg;
            lock (_cacheLock)
            {
                if (!_pendingRegistrations.TryGetValue(email, out pendingReg))
                {
                    return ServiceResult<bool>.FailureResult("No pending registration found for this email.", new List<string>());
                }

                // Kiểm tra thời gian gửi lại (chỉ cho phép gửi lại sau 2 phút)
                if (DateTime.UtcNow < pendingReg.ExpiresAt.AddMinutes(-8))
                {
                    return ServiceResult<bool>.FailureResult("Please wait before requesting a new verification code.", new List<string>());
                }
            }

            // Tạo OTP mới
            var newOtp = GenerateOTP();
            var newExpiryTime = DateTime.UtcNow.AddMinutes(10);

            lock (_cacheLock)
            {
                pendingReg.Otp = newOtp;
                pendingReg.ExpiresAt = newExpiryTime;
                pendingReg.Attempts = 0; // Reset attempts
            }

            // Gửi email OTP mới
            await _emailService.SendEmailAsync(
                email,
                "New Verification Code - LightStore",
                $"<h2>Email Verification</h2>" +
                $"<p>Your new verification code is: <strong>{newOtp}</strong></p>" +
                $"<p>This code will expire in 10 minutes.</p>" +
                $"<p>If you didn't request this, please ignore this email.</p>"
            );

            _logger.LogInformation("New registration OTP sent to email: {Email}", email);
            return ServiceResult<bool>.SuccessResult(true, "A new verification code has been sent to your email.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to resend registration OTP for {Email}", email);
            return ServiceResult<bool>.FailureResult("An error occurred while sending verification code.", new List<string> { ex.Message });
        }
    }

    /// <summary>
    /// Legacy RegisterAsync method - giữ để backward compatibility
    /// </summary>
    public async Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterDto registerDto)
    {
        // Redirect to new OTP-based flow
        var otpResult = await SendRegistrationOtpAsync(registerDto);
        if (!otpResult.Success)
        {
            return ServiceResult<AuthResponseDto>.FailureResult(otpResult.Message, otpResult.Errors);
        }

        return ServiceResult<AuthResponseDto>.FailureResult("Please verify your email to complete registration.", new List<string>());
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

    
    /// <summary>
    /// statistics for admin dashboard, total user
    /// </summary>
    /// <param name="fromDate"></param>
    /// <param name="toDate"></param>
    /// <returns></returns>
    public async Task<ServiceResult<int>> GetTotalCustomersCountAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var query = _context.Users.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(u => u.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(u => u.CreatedAt <= toDate.Value);

            var total = await query.CountAsync();

            return ServiceResult<int>.SuccessResult(total, "Total customers retrieved successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get total customers count");
            return ServiceResult<int>.FailureResult("An error occurred while retrieving customers count.", new List<string> { ex.Message });
        }
    }





    #endregion


    #region Admin - Customer Management

    public async Task<ServiceResult<PagedResult<UserDto>>> GetCustomersAsync(PagedRequest request)
    {
        try
        {
            // Lấy tất cả users theo page từ repo
            var usersQuery = _context.Users.AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                string searchLower = request.Search.ToLower();
                usersQuery = usersQuery.Where(u => u.Email.ToLower().Contains(searchLower)
                                                || u.FirstName.ToLower().Contains(searchLower)
                                                || u.LastName.ToLower().Contains(searchLower));
            }

            // Sort (ví dụ theo CreatedAt mặc định)
            if (!string.IsNullOrWhiteSpace(request.Sort))
            {
                if (request.Sort.ToLower() == "createdat_desc")
                    usersQuery = usersQuery.OrderByDescending(u => u.CreatedAt);
                else if (request.Sort.ToLower() == "createdat_asc")
                    usersQuery = usersQuery.OrderBy(u => u.CreatedAt);
            }
            else
            {
                usersQuery = usersQuery.OrderByDescending(u => u.CreatedAt);
            }

            // Paging
            int totalCount = await usersQuery.CountAsync();
            var users = await usersQuery
                .Skip((request.Page - 1) * request.Size)
                .Take(request.Size)
                .ToListAsync();

            // Map to UserDto
            var userDtos = users.Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Phone = u.Phone ?? "",
                UserType = u.UserType,
                CreatedAt = u.CreatedAt,
                Roles = u.UserRoles.Select(r => r.Role.Name)
            }).ToList();

            var pagedResult = new PagedResult<UserDto>
            {
                Items = userDtos,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.Size
            };

            return ServiceResult<PagedResult<UserDto>>.SuccessResult(pagedResult, "Customers retrieved successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get customers list");
            return ServiceResult<PagedResult<UserDto>>.FailureResult("An error occurred while retrieving customers.", new List<string> { ex.Message });
        }
    }

    public async Task<ServiceResult<UserDto>> GetCustomerByIdAsync(int customerId)
    {
        try
        {
            var user = await _userRepo.GetUserByIdAsync(customerId);
            if (user == null)
                return ServiceResult<UserDto>.FailureResult("Customer not found.", new List<string>());

            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.Phone ?? "",
                UserType = user.UserType,
                CreatedAt = user.CreatedAt,
                Roles = user.UserRoles.Select(r => r.Role.Name)
            };

            return ServiceResult<UserDto>.SuccessResult(userDto, "Customer retrieved successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get customer by ID {CustomerId}", customerId);
            return ServiceResult<UserDto>.FailureResult("An error occurred while retrieving customer.", new List<string> { ex.Message });
        }
    }

    public async Task<ServiceResult<bool>> DeleteCustomerAsync(int customerId)
    {
        try
        {
            var result = await _userRepo.DeleteUserAsync(customerId);
            if (!result)
                return ServiceResult<bool>.FailureResult("Customer not found or could not be deleted.", new List<string>());

            return ServiceResult<bool>.SuccessResult(true, "Customer deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete customer with ID {CustomerId}", customerId);
            return ServiceResult<bool>.FailureResult("An error occurred while deleting customer.", new List<string> { ex.Message });
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

    public string GenerateAccessToken(User user, IEnumerable<string> roles)
    {
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] 
            ?? throw new InvalidOperationException("JWT key not found"));

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("UserType", user.UserType)
        };

        // Add roles vào claim
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(GetJwtExpirationMinutes()),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            ),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }


    // public string GenerateAccessToken(User user)
    // {
    //     var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]
    //         ?? throw new InvalidOperationException("JWT key not found"));
    //     var tokenHandler = new JwtSecurityTokenHandler();

    //     var claims = new List<Claim>
    //     {
    //         new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
    //         new Claim(ClaimTypes.Email, user.Email),
    //         new Claim("UserType", user.UserType)
    //     };

    //     // Lấy roles từ UserRoles (EF navigation) hoặc từ service
    //     var roles = user.UserRoles?.Select(r => r.Role.Name).ToList() ?? new List<string>();

    //     foreach (var role in roles)
    //     {
    //         claims.Add(new Claim(ClaimTypes.Role, role));
    //         // Quan trọng: thêm claim role
    //     }

    //     var tokenDescriptor = new SecurityTokenDescriptor
    //     {
    //         Subject = new ClaimsIdentity(claims),
    //         Expires = DateTime.Now.AddMinutes(GetJwtExpirationMinutes()),
    //         SigningCredentials = new SigningCredentials(
    //             new SymmetricSecurityKey(key),
    //             SecurityAlgorithms.HmacSha256Signature
    //         ),
    //         Issuer = _configuration["Jwt:Issuer"],
    //         Audience = _configuration["Jwt:Audience"]
    //     };

    //     var token = tokenHandler.CreateToken(tokenDescriptor);
    //     return tokenHandler.WriteToken(token);
    // }


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