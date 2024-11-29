namespace ResultType;

public sealed record Error(string Code, string? Message = null);