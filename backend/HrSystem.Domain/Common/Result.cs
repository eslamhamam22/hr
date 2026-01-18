namespace HrSystem.Domain.Common;

/// <summary>
/// Generic result pattern wrapper for operations
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }

    public static Result<T> Success(T data) => new() { IsSuccess = true, Data = data };
    
    public static Result<T> Failure(string errorMessage) => 
        new() { IsSuccess = false, ErrorMessage = errorMessage };
}

/// <summary>
/// Non-generic result pattern wrapper
/// </summary>
public class Result
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }

    public static Result Success() => new() { IsSuccess = true };
    
    public static Result Failure(string errorMessage) => 
        new() { IsSuccess = false, ErrorMessage = errorMessage };
}
