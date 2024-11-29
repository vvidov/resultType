using Xunit;

namespace ResultType.Example;

public class UserRegistrationServiceTests
{
    private readonly UserRegistrationService _service = new();

    [Fact]
    public void WhenAllValidationsPass_ShouldRegisterUser()
    {
        // Arrange
        var email = "test@gmail.com";
        var password = "Pass123!@#";

        // Act
        var result = _service.RegisterUser(email, password);

        // Assert
        Assert.True(result.HasResult());
        Assert.Equal(email, result.Value.Email);
        Assert.NotNull(result.Value.HashedPassword);
        Assert.True((DateTime.UtcNow - result.Value.CreatedAt).TotalSeconds < 5);
    }

    [Theory]
    [InlineData("invalid-email", "Pass123!@#", "EMAIL_001", "Email format is invalid")]
    [InlineData("test@invalid.domain", "Pass123!@#", "EMAIL_002", "Email domain is not allowed")]
    [InlineData("test@gmail.com", "short", "PWD_001", "Password must be at least 8 characters")]
    [InlineData("test@gmail.com", "password123", "PWD_002", "Password must contain at least one number and one special character")]
    [InlineData("test@outlook.com", "onlyspecial!", "PWD_002", "Password must contain at least one number and one special character")]
    [InlineData("test@company.com", "12345678", "PWD_002", "Password must contain at least one number and one special character")]
    public void WhenValidationFails_ShouldReturnError(string email, string password, string expectedErrorCode, string expectedErrorMessage)
    {
        // Act
        var result = _service.RegisterUser(email, password);

        // Assert
        Assert.False(result.HasResult());
        Assert.Equal(expectedErrorCode, result.Error?.Code);
        Assert.Equal(expectedErrorMessage, result.Error?.Message);
    }

    [Theory]
    [InlineData("test@gmail.com")]
    [InlineData("user@outlook.com")]
    [InlineData("employee@company.com")]
    public void WhenUserAlreadyExists_ShouldReturnError(string email)
    {
        // Arrange
        var password = "Pass123!@#";

        // Act
        var firstResult = _service.RegisterUser(email, password);
        var secondResult = _service.RegisterUser(email, password);

        // Assert
        Assert.True(firstResult.HasResult());
        Assert.False(secondResult.HasResult());
        Assert.Equal("USER_001", secondResult.Error?.Code);
        Assert.Equal("User with this email already exists", secondResult.Error?.Message);
    }

    [Theory]
    [InlineData("TEST@GMAIL.COM", "test@gmail.com")]
    [InlineData("User@OUTLOOK.com", "user@outlook.com")]
    [InlineData("Employee@Company.COM", "employee@company.com")]
    public void WhenEmailHasDifferentCase_ShouldNormalizeToLowerCase(string inputEmail, string expectedEmail)
    {
        // Arrange
        var password = "Pass123!@#";

        // Act
        var result = _service.RegisterUser(inputEmail, password);

        // Assert
        Assert.True(result.HasResult());
        Assert.Equal(expectedEmail, result.Value.Email);
    }

    [Fact]
    public void WhenRegisteringMultipleUsers_ShouldSucceed()
    {
        // Arrange
        var users = new[]
        {
            ("user1@gmail.com", "Pass123!@#"),
            ("user2@outlook.com", "Secret456$%^"),
            ("user3@company.com", "Complex789&*(")
        };

        // Act & Assert
        foreach (var (email, password) in users)
        {
            var result = _service.RegisterUser(email, password);
            Assert.True(result.HasResult());
            Assert.Equal(email, result.Value.Email);
        }
    }

    [Theory]
    [InlineData("test@gmail.com", "Pass123!@#", true, true, true, null)] // Success case
    [InlineData("test@gmail.com", "Pass123!@#", false, true, false, "NOTIFY_001")] // Service unavailable
    [InlineData("test@gmail.com", "Pass123!@#", true, false, false, "NOTIFY_002")] // Template invalid
    [InlineData("test@gmail.com", "Pass123!@#", false, false, false, "NOTIFY_001")] // Both failed (should fail fast on service)
    public void WhenEmailServiceOrTemplateValidation_ShouldBehaveCorrectly(
        string email, 
        string password, 
        bool emailServiceAvailable,
        bool emailTemplateValid,
        bool shouldSucceed,
        string? expectedErrorCode)
    {
        // Arrange
        var service = new UserRegistrationService(emailServiceAvailable, emailTemplateValid);

        // Act
        var result = service.RegisterUser(email, password);

        // Assert
        Assert.Equal(shouldSucceed, result.HasResult());
        if (!shouldSucceed)
        {
            Assert.Equal(expectedErrorCode, result.Error?.Code);
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void WhenEmailIsEmptyOrNull_ShouldReturnError(string email)
    {
        // Arrange
        var password = "Pass123!@#";

        // Act
        var result = _service.RegisterUser(email, password);

        // Assert
        Assert.False(result.HasResult());
        Assert.Equal("EMAIL_001", result.Error?.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void WhenPasswordIsEmptyOrNull_ShouldReturnError(string password)
    {
        // Arrange
        var email = "test@gmail.com";

        // Act
        var result = _service.RegisterUser(email, password);

        // Assert
        Assert.False(result.HasResult());
        Assert.Equal("PWD_001", result.Error?.Code);
    }
}
