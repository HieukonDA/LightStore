using Moq;
using FluentAssertions;
using TheLightStore.Application.Exceptions;
using TheLightStore.Tests.Fixtures;
using static TheLightStore.Application.Dtos.AuthDto;

namespace TheLightStore.Tests.Unit.Services;

/// <summary>
/// Unit tests for AuthService
/// Each test method represents a specific scenario for the methods in AuthService
/// </summary>
public class AuthServiceTests : IDisposable
{
    private readonly AuthFixture _fixture;

    public AuthServiceTests()
    {
        _fixture = new AuthFixture();
        _fixture.SetupDefaultConfiguration();
    }

    public void Dispose()
    {
        _fixture.Reset();
    }

    #region Login Tests

    [Fact]
    public async Task Login_WithValidEmailAndPassword_ReturnsSuccessResponseWithTokens()
    {
        // Arrange
        var authService = _fixture.CreateAuthService();
        var validUser = _fixture.CreateValidUser("user-123", "test@example.com");
        var loginDto = new LoginDto { UserName = "test@example.com", Password = "ValidPassword123!" };

        _fixture.MockIdentityService
            .Setup(x => x.FindByNameAsync(loginDto.UserName))
            .ReturnsAsync(validUser);

        _fixture.MockIdentityService
            .Setup(x => x.CheckPasswordAsync(validUser.UserId!, loginDto.Password))
            .ReturnsAsync(true);

        _fixture.MockIdentityService
            .Setup(x => x.GetUserRoleAsync(validUser.UserId!))
            .ReturnsAsync("Customer");

        // Act
        var result = await authService.Login(loginDto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        var loginResponse = result.Data as LoginResponse;
        loginResponse?.AccessToken.Should().NotBeNullOrEmpty();
        loginResponse?.RefreshToken.Should().NotBeNullOrEmpty();
        loginResponse?.User?.UserId.Should().Be("user-123");
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ThrowsUnauthorizedException()
    {
        // Arrange
        var authService = _fixture.CreateAuthService();
        var validUser = _fixture.CreateValidUser("user-123", "test@example.com");
        var loginDto = new LoginDto { UserName = "test@example.com", Password = "WrongPassword" };

        _fixture.MockIdentityService
            .Setup(x => x.FindByNameAsync(loginDto.UserName))
            .ReturnsAsync(validUser);

        _fixture.MockIdentityService
            .Setup(x => x.CheckPasswordAsync(validUser.UserId!, loginDto.Password))
            .ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(
            () => authService.Login(loginDto)
        );
        exception.Message.Should().Contain("Invalid email or password");
    }

    [Fact]
    public async Task Login_WithInactiveUser_ThrowsForbiddenException()
    {
        // Arrange
        var authService = _fixture.CreateAuthService();
        var inactiveUser = _fixture.CreateValidUser("user-123", "test@example.com");
        inactiveUser.IsActive = false;
        var loginDto = new LoginDto { UserName = "test@example.com", Password = "ValidPassword123!" };

        _fixture.MockIdentityService
            .Setup(x => x.FindByNameAsync(loginDto.UserName))
            .ReturnsAsync(inactiveUser);

        _fixture.MockIdentityService
            .Setup(x => x.CheckPasswordAsync(inactiveUser.UserId!, loginDto.Password))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ForbiddenException>(
            () => authService.Login(loginDto)
        );
        exception.Message.Should().Contain("inactive");
    }

    [Fact]
    public async Task Login_WithNonexistentUser_ThrowsUnauthorizedException()
    {
        // Arrange
        var authService = _fixture.CreateAuthService();
        var loginDto = new LoginDto { UserName = "nonexistent@example.com", Password = "Password123!" };

        _fixture.MockIdentityService
            .Setup(x => x.FindByNameAsync(loginDto.UserName))
            .ReturnsAsync((MeDto?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(
            () => authService.Login(loginDto)
        );
        exception.Message.Should().Contain("Invalid email or password");
    }

    [Fact]
    public async Task Login_WithInvalidFormat_ThrowsBadRequestException()
    {
        // Arrange
        var authService = _fixture.CreateAuthService();
        var loginDto = new LoginDto { UserName = "invalidformat", Password = "Password123!" };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => authService.Login(loginDto)
        );
        exception.Message.Should().Contain("Invalid login format");
    }

    #endregion

    #region Register Tests

    [Fact]
    public async Task Register_WithValidEmail_CreatesUserAndCustomerSuccessfully()
    {
        // Arrange
        var authService = _fixture.CreateAuthService();
        var newUser = _fixture.CreateValidUser("new-user-id", "newuser@example.com");
        var customerType = _fixture.CreateValidCustomerType();
        var registerDto = new RegisterDto { UserName = "newuser@example.com", Password = "ValidPassword123!" };

        _fixture.MockIdentityService
            .Setup(x => x.UserExistsAsync(registerDto.UserName))
            .ReturnsAsync(false);

        _fixture.MockIdentityService
            .Setup(x => x.GetRoleByNameAsync("Customer"))
            .ReturnsAsync(1L);

        _fixture.MockIdentityService
            .Setup(x => x.CreateAsync(registerDto.UserName, registerDto.Password, registerDto.UserName, null))
            .ReturnsAsync((true, null));

        _fixture.MockIdentityService
            .Setup(x => x.FindByNameAsync(registerDto.UserName))
            .ReturnsAsync(newUser);

        _fixture.MockCodeService
            .Setup(x => x.GenerateOtpCode())
            .Returns("123456");

        _fixture.MockCacheService
            .Setup(x => x.Set(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()));

        _fixture.SetupEmailSendingSuccess();

        _fixture.MockAuthDataService
            .Setup(x => x.GetCustomerTypeByNameAsync("Khách Hàng Phổ Thông"))
            .ReturnsAsync(customerType);

        _fixture.MockCodeService
            .Setup(x => x.GenerateCustomerCodeAsync())
            .ReturnsAsync("CUST-001");

        _fixture.MockAuthDataService
            .Setup(x => x.BeginTransactionAsync())
            .Returns(Task.CompletedTask);

        _fixture.MockAuthDataService
            .Setup(x => x.AddCustomerAsync(It.IsAny<TheLightStore.Domain.Entities.Customers.Customer>()))
            .Returns(Task.CompletedTask);

        _fixture.MockAuthDataService
            .Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        _fixture.MockAuthDataService
            .Setup(x => x.CommitTransactionAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await authService.Register(registerDto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("Registration successful");
        _fixture.MockAuthDataService.Verify(x => x.BeginTransactionAsync(), Times.Once);
        _fixture.MockAuthDataService.Verify(x => x.CommitTransactionAsync(), Times.Once);
    }

    [Fact]
    public async Task Register_WithExistingEmail_ThrowsBadRequestException()
    {
        // Arrange
        var authService = _fixture.CreateAuthService();
        var registerDto = new RegisterDto { UserName = "existing@example.com", Password = "Password123!" };

        _fixture.MockIdentityService
            .Setup(x => x.UserExistsAsync(registerDto.UserName))
            .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => authService.Register(registerDto)
        );
        exception.Message.Should().Contain("Email already exists");
    }

    [Fact]
    public async Task Register_WithoutCustomerType_ThrowsBadRequestExceptionAndRollsBack()
    {
        // Arrange
        var authService = _fixture.CreateAuthService();
        var newUser = _fixture.CreateValidUser("new-user-id", "newuser@example.com");
        var registerDto = new RegisterDto { UserName = "newuser@example.com", Password = "ValidPassword123!" };

        _fixture.MockIdentityService
            .Setup(x => x.UserExistsAsync(registerDto.UserName))
            .ReturnsAsync(false);

        _fixture.MockIdentityService
            .Setup(x => x.GetRoleByNameAsync("Customer"))
            .ReturnsAsync(1L);

        _fixture.MockIdentityService
            .Setup(x => x.CreateAsync(registerDto.UserName, registerDto.Password, registerDto.UserName, null))
            .ReturnsAsync((true, null));

        _fixture.MockIdentityService
            .Setup(x => x.FindByNameAsync(registerDto.UserName))
            .ReturnsAsync(newUser);

        _fixture.MockCodeService
            .Setup(x => x.GenerateOtpCode())
            .Returns("123456");

        _fixture.MockCacheService
            .Setup(x => x.Set(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()));

        _fixture.SetupEmailSendingSuccess();

        _fixture.MockAuthDataService
            .Setup(x => x.BeginTransactionAsync())
            .Returns(Task.CompletedTask);

        _fixture.MockAuthDataService
            .Setup(x => x.GetCustomerTypeByNameAsync("Khách Hàng Phổ Thông"))
            .ReturnsAsync((TheLightStore.Domain.Entities.Customers.CustomerType?)null);

        _fixture.MockAuthDataService
            .Setup(x => x.RollbackTransactionAsync())
            .Returns(Task.CompletedTask);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => authService.Register(registerDto)
        );
        
        exception.Message.Should().Contain("CustomerType does not exist");
        _fixture.MockAuthDataService.Verify(x => x.RollbackTransactionAsync(), Times.Once);
    }

    #endregion

    #region Me Tests

    [Fact]
    public async Task Me_WithValidToken_ReturnsCurrentUserData()
    {
        // Arrange
        var authService = _fixture.CreateAuthService();
        var userId = "user-123";
        var validUser = _fixture.CreateValidUser(userId, "test@example.com");

        _fixture.MockCurrentUserService
            .Setup(x => x.GetCurrentUserId())
            .Returns(userId);

        _fixture.MockIdentityService
            .Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(validUser);

        _fixture.MockAuthDataService
            .Setup(x => x.GetEmployeeByUserIdAsync(userId))
            .ReturnsAsync((TheLightStore.Domain.Entities.Employees.Employee?)null);

        // Act
        var result = await authService.Me();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        var meDto = result.Data as MeDto;
        meDto?.UserId.Should().Be(userId);
        meDto?.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task Me_WithEmployeeUser_ReturnsUserWithEmployeeCode()
    {
        // Arrange
        var authService = _fixture.CreateAuthService();
        var userId = "employee-123";
        var validUser = _fixture.CreateValidUser(userId, "employee@example.com");
        var employee = new TheLightStore.Domain.Entities.Employees.Employee
        {
            Id = Guid.NewGuid(),
            Code = "EMP001",
            UserId = userId
        };

        _fixture.MockCurrentUserService
            .Setup(x => x.GetCurrentUserId())
            .Returns(userId);

        _fixture.MockIdentityService
            .Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(validUser);

        _fixture.MockAuthDataService
            .Setup(x => x.GetEmployeeByUserIdAsync(userId))
            .ReturnsAsync(employee);

        // Act
        var result = await authService.Me();

        // Assert
        result.IsSuccess.Should().BeTrue();
        var meDto = result.Data as MeDto;
        meDto?.EmployeeCode.Should().Be("EMP001");
    }

    [Fact]
    public async Task Me_WithoutToken_ReturnsErrorResponse()
    {
        // Arrange
        var authService = _fixture.CreateAuthService();

        _fixture.MockCurrentUserService
            .Setup(x => x.GetCurrentUserId())
            .Returns((string?)null);

        // Act
        var result = await authService.Me();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("User ID not found in token");
    }

    [Fact]
    public async Task Me_WithInvalidUserId_ReturnsErrorResponse()
    {
        // Arrange
        var authService = _fixture.CreateAuthService();

        _fixture.MockCurrentUserService
            .Setup(x => x.GetCurrentUserId())
            .Returns("invalid-user-id");

        _fixture.MockIdentityService
            .Setup(x => x.FindByIdAsync("invalid-user-id"))
            .ReturnsAsync((MeDto?)null);

        // Act
        var result = await authService.Me();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("User not found with ID");
    }

    #endregion

    #region ConfirmEmail Tests

    [Fact]
    public async Task ConfirmEmail_WithValidOtp_ActivatesUserAndRemovesOtp()
    {
        // Arrange
        var authService = _fixture.CreateAuthService();
        var email = "test@example.com";
        var otp = "123456";
        var validUser = _fixture.CreateValidUser("user-123", email);
        validUser.IsActive = false;

        _fixture.MockIdentityService
            .Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(validUser);

        _fixture.MockCacheService
            .Setup(x => x.Get(email, "OtpCode"))
            .Returns(otp);

        _fixture.MockIdentityService
            .Setup(x => x.UpdateAsync(It.IsAny<MeDto>()))
            .ReturnsAsync((true, null));

        _fixture.MockIdentityService
            .Setup(x => x.GetUserRoleAsync(validUser.UserId!))
            .ReturnsAsync("Customer");

        _fixture.MockCacheService
            .Setup(x => x.Remove(email, "OtpCode"));

        // Act
        var result = await authService.ConfirmEmail(otp, email);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("Email confirmation successful");
        _fixture.MockCacheService.Verify(x => x.Remove(email, "OtpCode"), Times.Once);
    }

    [Fact]
    public async Task ConfirmEmail_WithInvalidOtp_ThrowsBadRequestException()
    {
        // Arrange
        var authService = _fixture.CreateAuthService();
        var email = "test@example.com";
        var otp = "wrong-otp";
        var validUser = _fixture.CreateValidUser("user-123", email);

        _fixture.MockIdentityService
            .Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync(validUser);

        _fixture.MockCacheService
            .Setup(x => x.Get(email, "OtpCode"))
            .Returns("123456");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<BadRequestException>(
            () => authService.ConfirmEmail(otp, email)
        );
        
        exception.Message.Should().Contain("Invalid OTP");
    }

    [Fact]
    public async Task ConfirmEmail_WithNonexistentEmail_ThrowsUnauthorizedException()
    {
        // Arrange
        var authService = _fixture.CreateAuthService();
        var email = "nonexistent@example.com";
        var otp = "123456";

        _fixture.MockIdentityService
            .Setup(x => x.FindByEmailAsync(email))
            .ReturnsAsync((MeDto?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedException>(
            () => authService.ConfirmEmail(otp, email)
        );
        
        exception.Message.Should().Contain("Email not found");
    }

    #endregion

    #region ChangePassword Tests

    [Fact]
    public async Task ChangePassword_WithValidCurrentPassword_ChangesPasswordSuccessfully()
    {
        // Arrange
        var authService = _fixture.CreateAuthService();
        var userId = "user-123";
        var validUser = _fixture.CreateValidUser(userId);
        var changePasswordDto = new ChangePasswordDto
        {
            CurrentPassword = "CurrentPassword123!",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "NewPassword123!"
        };

        _fixture.MockCurrentUserService
            .Setup(x => x.GetCurrentUserId())
            .Returns(userId);

        _fixture.MockIdentityService
            .Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(validUser);

        _fixture.MockIdentityService
            .Setup(x => x.ChangePasswordAsync(userId, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword))
            .ReturnsAsync((true, null));

        // Act
        var result = await authService.ChangePassword(changePasswordDto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("Password changed successfully");
    }

    [Fact]
    public async Task ChangePassword_WithMismatchedPasswords_ReturnsBadRequestResponse()
    {
        // Arrange
        var authService = _fixture.CreateAuthService();
        var userId = "user-123";
        var validUser = _fixture.CreateValidUser(userId);
        var changePasswordDto = new ChangePasswordDto
        {
            CurrentPassword = "CurrentPassword123!",
            NewPassword = "NewPassword123!",
            ConfirmPassword = "DifferentPassword123!"
        };

        _fixture.MockCurrentUserService
            .Setup(x => x.GetCurrentUserId())
            .Returns(userId);

        _fixture.MockIdentityService
            .Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(validUser);

        // Act
        var result = await authService.ChangePassword(changePasswordDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Passwords do not match");
    }

    #endregion

    #region UpdateProfile Tests

    [Fact]
    public async Task UpdateProfile_WithValidData_UpdatesUserProfileSuccessfully()
    {
        // Arrange
        var authService = _fixture.CreateAuthService();
        var userId = "user-123";
        var validUser = _fixture.CreateValidUser(userId);
        var updateProfileDto = new UpdateProfileDto
        {
            FirstName = "John",
            LastName = "Doe",
            Address = "123 Main St"
        };

        _fixture.MockCurrentUserService
            .Setup(x => x.GetCurrentUserId())
            .Returns(userId);

        _fixture.MockIdentityService
            .Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(validUser);

        _fixture.MockIdentityService
            .Setup(x => x.UpdateAsync(It.IsAny<MeDto>()))
            .ReturnsAsync((true, null));

        // Act
        var result = await authService.UpdateProfile(updateProfileDto);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Contain("Profile updated successfully");
    }

    [Fact]
    public async Task UpdateProfile_WithoutAuthentication_ReturnsBadResponse()
    {
        // Arrange
        var authService = _fixture.CreateAuthService();
        var updateProfileDto = new UpdateProfileDto
        {
            FirstName = "John",
            LastName = "Doe",
            Address = "123 Main St"
        };

        _fixture.MockCurrentUserService
            .Setup(x => x.GetCurrentUserId())
            .Returns((string?)null);

        // Act
        var result = await authService.UpdateProfile(updateProfileDto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("not authenticated");
    }

    #endregion
}
