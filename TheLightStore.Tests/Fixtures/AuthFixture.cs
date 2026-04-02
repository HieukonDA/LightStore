using Moq;
using TheLightStore.Application.Interfaces.Infrastructures;
using TheLightStore.Application.Interfaces.Repositories;
using TheLightStore.Application.Interfaces.Services;
using TheLightStore.Application.Services.SysServices;
using TheLightStore.Domain.Entities.Customers;
using static TheLightStore.Application.Dtos.AuthDto;

namespace TheLightStore.Tests.Fixtures;

/// <summary>
/// Fixture for AuthService testing - provides mocked dependencies and test setup
/// </summary>
public class AuthFixture
{
    public Mock<IIdentityService> MockIdentityService { get; set; } = new();
    public Mock<IConfigurationService> MockConfigurationService { get; set; } = new();
    public Mock<ICurrentUserService> MockCurrentUserService { get; set; } = new();
    public Mock<IEmailSenderService> MockEmailSenderService { get; set; } = new();
    public Mock<ICacheService> MockCacheService { get; set; } = new();
    public Mock<ICodeService> MockCodeService { get; set; } = new();
    public Mock<IAuthDataService> MockAuthDataService { get; set; } = new();

    public AuthService CreateAuthService()
    {
        return new AuthService(
            MockIdentityService.Object,
            MockConfigurationService.Object,
            MockCurrentUserService.Object,
            MockEmailSenderService.Object,
            MockCacheService.Object,
            MockCodeService.Object,
            MockAuthDataService.Object
        );
    }

    /// <summary>
    /// Setup default configuration values
    /// </summary>
    public void SetupDefaultConfiguration()
    {
        MockConfigurationService.Setup(x => x.GetJwtKey())
            .Returns("4335d179-a729-489c-82ec-b5ccd05a10f5");
        
        MockConfigurationService.Setup(x => x.GetJwtRefreshKey())
            .Returns("e8DsXj00N2D6w47+8YMYKQZMKfiL2poOzDAB9OxHUZw=");
        
        MockConfigurationService.Setup(x => x.GetJwtIssuer())
            .Returns("YourAppName");
        
        MockConfigurationService.Setup(x => x.GetJwtAudience())
            .Returns("YourAppUsers");
        
        MockConfigurationService.Setup(x => x.GetConfirmEmailUrl())
            .Returns("http://localhost:3000/confirm-email");
        
        MockConfigurationService.Setup(x => x.GetOtpExpiryMinutes())
            .Returns(15);
    }

    /// <summary>
    /// Setup a valid user for testing
    /// </summary>
    public MeDto CreateValidUser(string userId = "test-user-id", string email = "test@example.com")
    {
        return new MeDto
        {
            UserId = userId,
            UserName = email,
            Email = email,
            FirstName = "Test",
            LastName = "User",
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Setup a valid customer type for testing
    /// </summary>
    public CustomerType CreateValidCustomerType(long id = 1)
    {
        return new CustomerType
        {
            Id = id,
            Name = "Khách Hàng Phổ Thông",
            Points = 0,
            Description = "Regular Customer",
            PercentDiscount = 5,
            Code = "KHP",
            IsActive = true
        };
    }

    /// <summary>
    /// Setup email sending mock
    /// </summary>
    public void SetupEmailSendingSuccess()
    {
        MockEmailSenderService.Setup(x => x.SendMailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()
        )).ReturnsAsync(new Domain.Commons.Models.ResponseResult
        {
            IsSuccess = true,
            Message = "Email sent successfully"
        });
    }

    /// <summary>
    /// Setup email sending failure
    /// </summary>
    public void SetupEmailSendingFailure(string errorMessage = "Email service unavailable")
    {
        MockEmailSenderService.Setup(x => x.SendMailAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()
        )).ReturnsAsync(new Domain.Commons.Models.ResponseResult
        {
            IsSuccess = false,
            Message = errorMessage
        });
    }

    /// <summary>
    /// Reset all mocks
    /// </summary>
    public void Reset()
    {
        MockIdentityService.Reset();
        MockConfigurationService.Reset();
        MockCurrentUserService.Reset();
        MockEmailSenderService.Reset();
        MockCacheService.Reset();
        MockCodeService.Reset();
        MockAuthDataService.Reset();
    }
}
