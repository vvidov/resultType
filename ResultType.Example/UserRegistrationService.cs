using System.Text.RegularExpressions;

namespace ResultType.Example;

public record UserData(string Email, string Password);
public record User(string Email, string HashedPassword, DateTime CreatedAt);

public class UserRegistrationService
{
    private static readonly string[] AllowedDomains = { "gmail.com", "outlook.com", "company.com" };
    private readonly HashSet<string> _existingUsers = new();
    private readonly bool _emailServiceAvailable;
    private readonly bool _emailTemplateValid;

    public UserRegistrationService(bool emailServiceAvailable = true, bool emailTemplateValid = true)
    {
        _emailServiceAvailable = emailServiceAvailable;
        _emailTemplateValid = emailTemplateValid;
    }

    public Result<User, Error> RegisterUser(string email, string password)
    {
        return ValidateEmail(email)
            .OnSuccess(validEmail => ValidatePassword(password)
                .OnSuccess(validPassword => CreateUser(new UserData(validEmail, validPassword)))
                .OnSuccess(user => SendWelcomeEmail(user)));
    }

    private Result<string, Error> ValidateEmail(string email)
    {
        // Validation 1: Check email format
        if (string.IsNullOrWhiteSpace(email) || !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            return UserRegistrationErrors.InvalidEmailFormat;

        // Validation 2: Check allowed domains
        var domain = email.Split('@')[1].ToLower();
        if (!AllowedDomains.Contains(domain))
            return UserRegistrationErrors.EmailDomainNotAllowed;

        return email.ToLower(); // Normalize email to lowercase
    }

    private Result<string, Error> ValidatePassword(string password)
    {
        // Validation 1: Check password length
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            return UserRegistrationErrors.PasswordTooShort;

        // Validation 2: Check password requirements (number + special char)
        if (!Regex.IsMatch(password, @"^(?=.*[0-9])(?=.*[^a-zA-Z0-9]).+$"))
            return UserRegistrationErrors.PasswordMissingRequirements;

        return password;
    }

    private Result<User, Error> CreateUser(UserData userData)
    {
        // Validation 1: Check if user exists
        var email = userData.Email.ToLower();
        if (_existingUsers.Contains(email))
            return UserRegistrationErrors.UserAlreadyExists;

        // Validation 2: Validate user data
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(userData.Password))
            return UserRegistrationErrors.InvalidUserData;

        var user = new User(
            email,
            HashedPassword: HashPassword(userData.Password),
            CreatedAt: DateTime.UtcNow
        );

        _existingUsers.Add(email);
        return user;
    }

    private Result<User, Error> SendWelcomeEmail(User user)
    {
        // Validation 1: Check if email service is available
        if (!_emailServiceAvailable)
            return UserRegistrationErrors.EmailServiceUnavailable;

        // Validation 2: Check if email template is valid
        if (!_emailTemplateValid)
            return UserRegistrationErrors.EmailTemplateInvalid;

        // Send welcome email (simulated)
        Console.WriteLine($"Welcome email sent to {user.Email}");
        return user;
    }

    // Helper methods (simulated)
    private string HashPassword(string password) => 
        Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
}
