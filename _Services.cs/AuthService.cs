


namespace TheLightStore.Services;

public class AuthService : IAuthService
{
    readonly private DBContext _context;
    readonly private IUserRepo _userRepo;
    readonly private ILogger<AuthService> _logger;
    readonly private IConfiguration _configuration;

    public AuthService(DBContext context, IUserRepo userRepo, ILogger<AuthService> logger, IConfiguration configuration)
    {
        _context = context;
        _userRepo = userRepo;
        _logger = logger;
        _configuration = configuration;
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
                ExpireAt = DateTime.Now.AddMinutes(GetJwtExpirationMinutes())
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
                User = new User
                {
                    Id = newUser.Id,
                    Email = newUser.Email,
                    UserType = newUser.UserType
                }
            };

            _logger.LogInformation("User registered successfully with email: {Email}", registerDto.Email);
            return  ServiceResult<AuthResponseDto>.SuccessResult(response);

        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "An error occurred while registering a new user.");
            return  ServiceResult<AuthResponseDto>.FailureResult("An error occurred while processing your request.", new List<string>());
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
        await Task.CompletedTask;
    }

    private int GetJwtExpirationMinutes()
    {
        return int.TryParse(_configuration["Jwt:ExpirationMinutes"], out var minutes) ? minutes : 60;
    }
    

    #endregion
}