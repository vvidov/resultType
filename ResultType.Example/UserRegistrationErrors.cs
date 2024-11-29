namespace ResultType.Example;

public static class UserRegistrationErrors
{
    // Email validation errors
    public static readonly Error InvalidEmailFormat = new("EMAIL_001", "Email format is invalid");
    public static readonly Error EmailDomainNotAllowed = new("EMAIL_002", "Email domain is not allowed");

    // Password validation errors
    public static readonly Error PasswordTooShort = new("PWD_001", "Password must be at least 8 characters");
    public static readonly Error PasswordMissingRequirements = new("PWD_002", "Password must contain at least one number and one special character");

    // User creation errors
    public static readonly Error UserAlreadyExists = new("USER_001", "User with this email already exists");
    public static readonly Error InvalidUserData = new("USER_002", "User data is incomplete or invalid");

    // Notification errors
    public static readonly Error EmailServiceUnavailable = new("NOTIFY_001", "Email service is currently unavailable");
    public static readonly Error EmailTemplateInvalid = new("NOTIFY_002", "Welcome email template is invalid");
}
